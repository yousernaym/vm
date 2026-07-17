# Sidwiz pitch split: replace per-frame slot heuristics with MIDI-guided per-voice audio separation

## Context

The `sidwiz-stability` branch splits a sidwiz channel into up to 4 waveform slots by pitch. Arpeggios (rapid sequential notes) split correctly, but the user wants that behavior **removed** (undesirable visuals). Sustained concurrent notes — the case that should work — don't: every slot renders the **same mixed samples**, differing only by trigger position, so the per-frame slot-allocation heuristics (`PidStat`, onset/layer lanes, arp detection, eviction) fight over which pitch keys which slot, yielding one merged waveform or slots alternating with frozen curves. The `MaxVoiceStep = 7` constant also wrongly splits a monophonic melody jumping >7 semitones into two slots.

Fix per the user's attached algorithm document: precompute **one separated audio buffer per voice** via MIDI-guided STFT soft masking (assumes single-instrument per-track WAVs from "one track per instrument" + TrackAudio import). Each slot then renders its own buffer with its own trigger — no per-frame allocation at all. Voice assignment happens once over the MIDI: a note goes to a free voice, concurrent notes get different voices. Consequences: monophonic melodies are always one voice regardless of jump size (kills `MaxVoiceStep`), sequential arp notes all land in voice 0 (arp splitting disappears naturally), and audio-only pitch detection is dropped entirely (a track without MIDI notes never splits).

**User decisions:** STFT pipeline = simplified core **plus** global tuning estimate and learned harmonic template (NNLS etc. deferred to a documented phase 2). Polyphony overflow merges the extra note into the nearest-pitch voice. While separation computes, render unsplit with a label suffix ("(separating…)"); export waits for separation.

**Verified facts:** LibSidWiz compiles into VM.exe (same assembly; the "separate assembly" comment in Channel.cs is stale). All trigger algorithms and `RenderWave` read samples solely via `Channel.GetSample(int, bool forTrigger)` — one indirection routes everything. Rendering is single-threaded per frame (worker thread in preview; export thread inline when `WaveformPanel.Synchronous`, set in [SongRenderer.cs:716](VisualMusic/Controls/SongRenderer.cs#L716)). The export clone shares Channel objects (`Project.Clone(shareAudioProps: true)`, [Project.cs:2270](VisualMusic/Project.cs#L2270)) and runs `RefreshSidWizChannels()` on the export thread each frame. `NAudio.Dsp.FastFourierTransform` (NAudio.Core 2.3.0, already referenced) provides the FFT. `SampleBuffer._decoded` is immutable after load → safe cross-thread reads. `CandidateScratch`/`SelectionTolerance` in CandidateTrigger.cs are used only by the code being deleted.

## Changes by file

### 1. [ChannelSplit.cs](VisualMusic/tparty/LibSidWiz/ChannelSplit.cs) — new data model

- **Delete** `PitchSegment` struct and `PitchSegmentSource` delegate.
- **Trim `WaveSlot`**: delete `PitchId`, `RevealedLayer`; keep `LastTrigger`, `LastUpdateFrameStart`, `LastActiveEndSample`, `HasCurve`, `VisibleThisFrame`, `Bounds`. Keep `SplitLayout` enum.
- **Add**:
  - `ChannelVoiceSet` (immutable, published whole): `object Key` (opaque identity compared app-side), `VoiceAudio[] Voices`.
  - `VoiceAudio`: `float[] Samples` (full track length, same normalized domain as the channel buffer), sorted non-overlapping note spans `int[] SpanStarts`/`SpanEnds` (note intervals + release), with binary-search helpers `int LastActiveEndBefore(int cap)` and `bool SoundsIn(int start, int end)`.

### 2. [Channel.cs](VisualMusic/tparty/LibSidWiz/Channel.cs) — gut heuristics, add voice plumbing

- **Delete** (~lines 704–1313): `PitchTolerance`, `MaxVoiceStep`, `PitchContextSeconds`, `ArpRecurSeconds`, `PidStat`, `PitchSegments` property, all split scratch lists, `SegStartSample`/`SegEndSample(Raw)`, `FindPidGroup`, `Claim`, `ResolveSlots`, `SegBelongsToSlot`, `CollectPitchCandidates`, `FindSlotTriggerNonCandidate`, `FallbackHopMs`, `BuildDetectedSegments`, `DetectPitchId`. Trim `ResetSlots` (no `PitchId`/`RevealedLayer`).
- **Keep**: `Slots`/`BuildSlots`, `LayoutRowsThisFrame`/`LayoutRows`, `SplitCount`/`SplitLayout`, `GetSlotPen`/`SlotColor`/`Lerp`, unsplit `GetTriggerPoint`.
- **Add**:
  - `private volatile ChannelVoiceSet _voiceSet;` + public `VoiceSet` property (written by app thread, one volatile read per frame).
  - Render-thread frame latch `_frameVoices` + `internal bool SplitPreparedThisFrame => SplitCount > 1 && _frameVoices != null;` + `internal float[] FrameVoiceSamples(int k)`.
  - Sample routing: `private float[] _activeVoiceSamples; internal void SetActiveVoice(float[] s)`. In `GetSample`: if `_activeVoiceSamples != null`, read it (with `Scale` and `InvertedTrigger` applied as today) instead of `_samples`/`_samplesForTrigger`. `Scale` at read time keeps voice waveforms tracking the auto-scaler; external trigger files don't apply to slots (comment it).
  - `public string LabelSuffix` (render-only, never serialized; fires `Changed` so the template rebuilds) + `internal string EffectiveLabel`.
  - `public float[] DecodedSamples => _samples?.Decoded;` for the separation job.
- **Rewrite `UpdateSlots(frameStart, frameSamples)`**: latch `_voiceSet` (must match `Slots.Length`, else render unsplit: `LayoutRowsThisFrame = 1`, all slots invisible). Per slot k: update `LastActiveEndSample` from `voice.LastActiveEndBefore(failEnd)`; if `voice.SoundsIn(frameStart, failEnd)` run `UpdateSlotTrigger` with `SetActiveVoice(voice.Samples)` (try/finally reset); visibility = `HasCurve && frameStart - LastActiveEndSample <= ActivityLookaheadSeconds*SampleRate`; `LayoutRowsThisFrame` = visible count in Separate layout.
- **Rewrite `UpdateSlotTrigger`**: just the standard trigger path over the voice buffer (`Algorithm.GetTriggerPoint(this, frameStart, normalEnd, frameSamples, prev)` + failure-lookahead retry), `prev` reconstructed from `LastTrigger + (frameStart - LastUpdateFrameStart) - frameSamples` so the trigger advances smoothly; on failure hold the previous curve. No candidate pooling / segment gating — a voice buffer is silent outside its notes, so it behaves like a normal channel.

### 3. [SampleBuffer.cs](VisualMusic/tparty/LibSidWiz/SampleBuffer.cs)

Add `internal float[] Decoded => _decoded;` (immutable after load).

### 4. [WaveformRenderer.cs](VisualMusic/tparty/LibSidWiz/WaveformRenderer.cs)

- Template signature (~line 299): `c.Label` → `c.EffectiveLabel`; label draw in `DrawChannelTemplate` (~line 731) likewise.
- Slot-rendering gates (~line 341, plus `DrawChannelTemplate` zero-line/border branch and `VisibleSlotMask`): `ch.SplitCount > 1` → `ch.SplitPreparedThisFrame`. Unprepared split channels fall through to the normal single-wave path — that *is* the render-unsplit-while-separating behavior. `VisibleSlotMask` returns 0 when unprepared.
- In the slot draw loop, wrap `RenderWave` with `ch.SetActiveVoice(ch.FrameVoiceSamples(k))` / `finally SetActiveVoice(null)`.
- `PrepareFrame`/`ResetActivityState`/layout code otherwise unchanged.

### 5. [CandidateTrigger.cs](VisualMusic/tparty/LibSidWiz/Triggers/CandidateTrigger.cs)

Delete `CandidateScratch`, `SelectionTolerance`, and the split-path comment (only consumers are deleted). `TriggerCandidateSelector` itself is untouched (normal path uses it).

### 6. New file `VisualMusic/SidWizNoteSeparation.cs` (and **delete** [SidWizPitchSegments.cs](VisualMusic/SidWizPitchSegments.cs))

Static class, two entry points; header comment lists the phase-2 deferrals.

Knobs: `SequentialOverlapSec = 0.020`, `ReleaseSec = 0.15`, `FftSize = 4096` (`FftBits = 12`), `Hop = 1024`, `MaskPower = 1.7`, `HarmonicSigmaCents = 30`, tuning search ±100 cents step 5, `MaxHarmonics = 16` (capped below Nyquist).

**`BuildVoices(Project, Midi.Track, int voiceCount)`** — UI thread, fast. Maps ticks → audio seconds via `project.TicksToSeconds(tick + PlaybackOffsetT) - Props.PlaybackOffsetS` (same mapping the deleted `SidWizPitchSegments.Build` used). Sort notes by start (ties: ascending pitch). Sweep: a voice is *free* if all its active notes end within `startSec + SequentialOverlapSec` (≤20 ms overlap counts as sequential → fast arps walk through one voice); pick the free voice with nearest last pitch (final tie → lowest index, so fresh chords stack ascending pitch → ascending slot); **no free voice** → merge into the sounding voice with nearest active pitch (its template later sums its notes; the slot shows the sum). Returns `List<VoiceNote>[]` or null if no valid notes.

**`Separate(float[] samples, int sampleRate, voices, object key, CancellationToken)`** — background thread, seconds per track:
1. Hann-windowed frames on an absolute grid (frame f = `[f*Hop, f*Hop + FftSize)`); analysis + synthesis windowing with a shared per-sample `Σw²` normalization buffer so overlap-add output aligns 1:1 with the source. Sanity-check the NAudio FFT round trip (forward scales by 1/N, inverse doesn't) with a generated sine first.
2. Per-frame active-note lists per voice over `[StartSec, EndSec + ReleaseSec)` (release extension keeps decay tails in the note's voice).
3. **Global tuning** (doc step 4): on frames with ≤2 concurrent notes, score cent offsets by summed magnitude near predicted harmonics, weight `1/√h`; keep argmax (compute those frames' `|X|` once).
4. **Shared harmonic template `H[h]`** (doc step 5): median over single-note frames of magnitudes near each harmonic normalized to h=1; fallback `1/h` if <~20 usable frames.
5. **Per frame**: skip if no voice active; **fast path** mask=1 if exactly one voice active; else per voice `P_v[bin] = Σ_notes (vel/127)·Σ_h H[h]·exp(−Δcents²/(2σ²))` (touch only bins within ~3σ of each harmonic), amplitude `a_v` = weighted energy of `|X|` under normalized `P_v`, `mask_v = (a_v·P_v)^1.7 / (Σ_u … + ε)`, voice spectrum = `mask_v · X` (bins 0..N/2, mirror conjugate), inverse FFT, ×window, accumulate into `voiceBuf[v]`. Cancellation check every ~64 frames.
6. Divide buffers by the `Σw²` norm buffer; build per-voice merged spans; return `ChannelVoiceSet`.

### 7. [Project.cs](VisualMusic/Project.cs) — orchestration

- **Delete** `_pitchSegmentSources`/`_pitchSegSong`/`_pitchSegOffsetT`/`_pitchSegOffsetS` and `PushPitchSegmentSource` (~lines 1184–1222); replace its call in `RefreshSidWizChannels` (~line 1143) with `PushVoiceSet(tv, tp, ch, splitCount)`.
- **Add** `SidWizVoiceKey` (song ref, track number, offsets, voice count, decoded-array reference — equality by reference for song/samples, value for the rest), `SeparationJob { Key, Cts, Task }`, `Dictionary<int, SeparationJob> _separationJobs` (keyed by track number; single caller thread per Project instance — UI thread in preview, export thread during export — so no locking), optional `_lastFailedKey` to avoid retrying a faulted key each frame.
- **`PushVoiceSet` logic**:
  1. Not desired (`splitCount <= 1`, no notes, audio not loaded) → cancel/remove job, `ch.VoiceSet = null`, clear suffix. (Track without notes never splits — audio-only fallback dropped by design.)
  2. `ch.VoiceSet` already matches the key → clear suffix, drop finished job, done.
  3. Job with equal key: completed → publish `ch.VoiceSet = result`, clear suffix; faulted → log, remember failed key, clear suffix (stays unsplit); running → suffix `" (separating…)"`, and **if the waveform panel is in `Synchronous` (export) mode, `job.Task.Wait()` inline** then handle as completed/faulted — this guarantees no exported frame shows the unsplit fallback, including mid-export keyframed changes, with no `SongRenderer` changes. (Reach the panel the same way `RefreshSidWizChannels`' ecosystem does — via the draw host / owner that exposes `WaveformPanel`; pick the cleanest existing path during implementation.)
  4. Else: cancel stale job; skip if key equals `_lastFailedKey`; `BuildVoices` (null → treat as step 1); `Task.Run(() => Separate(ch.DecodedSamples, ch.SampleRate, voices, key, ct))`, store job, set suffix.
- Invalidation is implicit via key equality: song reference, playback offsets, split count (keyframeable — a change cancels + recomputes; preview shows unsplit + suffix meanwhile, export waits inline; comment the churn caveat), and audio identity by decoded-array reference (reload → new array → recompute).
- Comment the memory cost near `SeparationJob`: V full-track float buffers per split track (3-min 44.1 kHz at V=4 ≈ 127 MB); `short[]` storage is a phase-2 follow-up. Orphaned jobs on project replacement just run out and get GC'd (comment, no teardown).

## Implementation order

1. `SampleBuffer.Decoded` + `Channel` additions (VoiceSet, latch, SetActiveVoice/GetSample routing, LabelSuffix, DecodedSamples).
2. `ChannelSplit.cs` rewrite; 3. `Channel.cs` deletions + `UpdateSlots`/`UpdateSlotTrigger` rewrite; 4. `WaveformRenderer.cs` gating/label; 5. `CandidateTrigger.cs` cleanup → **solution compiles here**, split tracks render unsplit.
6. `SidWizNoteSeparation.cs` (BuildVoices, then Separate with FFT sanity check); 7. `Project.cs` orchestration + delete `SidWizPitchSegments.cs`.

Build: `msbuild d:\dev\vm\VisualMusic.sln /p:Configuration=Debug /p:Platform=x64` (via vswhere path per root CLAUDE.md); output in `VisualMusic\bin\Debug\net10.0-windows10.0.26100.0\VM.exe`.

## Phase 2 (done in SidWizNoteSeparation.cs)

NNLS per-voice amplitudes; temporal amplitude smoothing; per-register harmonic templates; residual redistributed among active voices; `short[]` (Q15) voice buffers.

Still deferred from the algorithm doc: MIDI↔WAV onset alignment; pitch-bend/vibrato; synthetic carrier display mode.

## Verification (manual)

Import a SID/MOD with "one track per instrument" + per-track audio (TrackAudio); enable the waveform view; set Pitch split count on the Audio tab.

1. **Separating indicator**: after enabling split, one normal waveform + "(separating…)" suffix; suffix clears and split appears when the task finishes.
2. **Monophonic melody with >7-semitone jumps**: stays in ONE slot, no frozen duplicate (the old MaxVoiceStep bug).
3. **Fast arpeggio**: ONE alternating waveform in one slot — must NOT split per pitch.
4. **Sustained chord (2–4 held notes)**: one slot per note, each live/moving (not frozen), each with its own period; check Stacked, Overlaid, Separate layouts.
5. **Polyphony overflow** (more notes than split count): extras merge into the nearest-pitch slot (summed waveform), no flicker.
6. **Track without notes**: never splits, no stuck suffix.
7. **Seek backwards** during playback: slots reset cleanly, refill going forward.
8. **Settings churn**: split count 2→3→2, HighPassFilter toggle (audio reload), playback-offset change — each exactly one recompute, no stuck suffix.
9. **Export** a short clip with a split track (also immediately after changing split count → inline wait path): no "(separating…)" in the output, split matches preview.
10. **Perf/memory**: 3-min track at V=4 → ~+130 MB working set; UI responsive during separation.
