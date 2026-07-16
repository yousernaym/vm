using System;
using System.Collections.Generic;
using System.Threading;
using LibSidWiz;
using NAudio.Dsp;

namespace VisualMusic
{
    /// <summary>
    /// MIDI-guided per-voice audio separation for the SidWiz pitch split. Two stages:
    ///   1. <see cref="BuildVoices"/> (UI thread): assigns each MIDI note to a voice so that
    ///      concurrent notes land on different voices while a monophonic line (or an arpeggio, whose
    ///      notes are sequential) stays on one voice.
    ///   2. <see cref="Separate"/> (background thread): STFT soft-mask separation of the track's mixed
    ///      audio into one buffer per voice, guided by the notes assigned to each voice.
    ///
    /// This is the "simplified core + global tuning + learned harmonic template" of the reference
    /// algorithm. Deferred to a future phase (intentionally NOT implemented here):
    ///   - NNLS per-voice amplitude estimation (replacing the weighted-energy estimate).
    ///   - Temporal smoothing of per-voice amplitudes across frames.
    ///   - Per-register (pitch-range) harmonic templates instead of one shared H[h].
    ///   - A residual "everything unexplained" track.
    ///   - short[] voice-buffer storage to halve memory.
    /// </summary>
    static class SidWizNoteSeparation
    {
        // Consecutive notes that overlap by less than this are treated as sequential (so slightly
        // overlapping tracker arps still walk through a single voice).
        const double SequentialOverlapSec = 0.020;
        // Each note's masking/activity interval extends this far past note-off so decay tails follow it.
        const double ReleaseSec = 0.15;

        const int FftSize = 4096;
        const int FftBits = 12;     // log2(FftSize)
        const int Hop = 1024;
        const double MaskPower = 1.7;
        const double HarmonicSigmaCents = 30.0;
        const double TuningRangeCents = 100.0;
        const double TuningStepCents = 5.0;
        const int MaxHarmonics = 16;
        const double Eps = 1e-9;

        // Cap how many frames feed the (cheap) tuning/template estimates.
        const int MaxEstimateFrames = 400;

        internal struct VoiceNote
        {
            public double StartSec, EndSec;
            public int Pitch, Velocity;
        }

        /// <summary>
        /// Assigns each of the track's notes to one of <paramref name="voiceCount"/> voices. Returns
        /// null if the track has no usable notes. Runs on the UI thread (uses Project.TicksToSeconds).
        /// </summary>
        internal static List<VoiceNote>[] BuildVoices(Project project, Midi.Track track, int voiceCount)
        {
            var notes = track?.Notes;
            if (notes == null || notes.Count == 0 || voiceCount < 1)
                return null;

            double offsetT = project.PlaybackOffsetT;
            double offsetS = project.Props.PlaybackOffsetS;

            var list = new List<VoiceNote>(notes.Count);
            foreach (var n in notes)
            {
                if (n.stop <= n.start || n.pitch < 0 || n.pitch > 127)
                    continue;
                double startSec = project.TicksToSeconds(n.start + offsetT) - offsetS;
                double endSec = project.TicksToSeconds(n.stop + offsetT) - offsetS;
                if (endSec <= startSec)
                    continue;
                list.Add(new VoiceNote { StartSec = startSec, EndSec = endSec, Pitch = n.pitch, Velocity = n.velocity });
            }
            if (list.Count == 0)
                return null;

            // Ascending start; simultaneous onsets in ascending pitch (so a fresh chord stacks
            // low→high across ascending voice indices).
            list.Sort((a, b) => a.StartSec != b.StartSec ? a.StartSec.CompareTo(b.StartSec) : a.Pitch.CompareTo(b.Pitch));

            var voices = new List<VoiceNote>[voiceCount];
            for (int v = 0; v < voiceCount; ++v)
                voices[v] = new List<VoiceNote>();

            var lastEnd = new double[voiceCount];
            var lastPitch = new int[voiceCount];
            var hasHistory = new bool[voiceCount];
            for (int v = 0; v < voiceCount; ++v)
                lastEnd[v] = double.NegativeInfinity;

            foreach (var note in list)
            {
                // A voice is free if its last note ended (within the overlap tolerance) before this
                // note starts. Pick the free voice whose last pitch is nearest (fresh voices lose ties,
                // and the ascending loop breaks final ties toward the lowest index).
                int chosen = -1;
                int bestDiff = int.MaxValue;
                for (int v = 0; v < voiceCount; ++v)
                {
                    if (lastEnd[v] > note.StartSec + SequentialOverlapSec)
                        continue; // busy
                    int diff = hasHistory[v] ? Math.Abs(lastPitch[v] - note.Pitch) : int.MaxValue - 1;
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        chosen = v;
                    }
                }
                if (chosen < 0)
                {
                    // Polyphony exceeds the voice count: merge into the sounding voice whose pitch is
                    // nearest (its slot then shows the sum of its overlapping notes).
                    int bestMergeDiff = int.MaxValue;
                    for (int v = 0; v < voiceCount; ++v)
                    {
                        int diff = Math.Abs(lastPitch[v] - note.Pitch);
                        if (diff < bestMergeDiff)
                        {
                            bestMergeDiff = diff;
                            chosen = v;
                        }
                    }
                    if (chosen < 0)
                        chosen = 0;
                }

                voices[chosen].Add(note);
                if (note.EndSec > lastEnd[chosen])
                    lastEnd[chosen] = note.EndSec;
                lastPitch[chosen] = note.Pitch;
                hasHistory[chosen] = true;
            }
            return voices;
        }

        /// <summary>
        /// Separates the mixed <paramref name="samples"/> into one buffer per voice via MIDI-guided
        /// STFT soft masking. Runs on a background thread; <paramref name="ct"/> aborts it.
        /// </summary>
        internal static ChannelVoiceSet Separate(float[] samples, int sampleRate, List<VoiceNote>[] voices,
            object key, CancellationToken ct)
        {
            int len = samples.Length;
            int V = voices.Length;
            int half = FftSize / 2;
            double binHz = (double)sampleRate / FftSize;
            double nyquist = sampleRate / 2.0;

            var w = new double[FftSize];
            for (int i = 0; i < FftSize; ++i)
                w[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (FftSize - 1)));

            var voiceBuf = new float[V][];
            for (int v = 0; v < V; ++v)
                voiceBuf[v] = new float[len];

            if (len < FftSize)
                return BuildResult(key, voices, voiceBuf, sampleRate, len);

            int frameCount = (len - FftSize) / Hop + 1;
            var norm = new double[len];

            double tuningCents = EstimateTuning(samples, w, sampleRate, binHz, nyquist, voices, frameCount, ct);
            double[] H = EstimateTemplate(samples, w, sampleRate, binHz, nyquist, voices, frameCount, tuningCents, ct);

            // Reusable per-frame buffers.
            var fft = new Complex[FftSize];
            var voiceSpec = new Complex[FftSize];
            var mag = new double[half + 1];
            var denom = new double[half + 1];
            var activeVoices = new List<int>(V);
            var noteScratch = new List<VoiceNote>[V];
            var P = new double[V][];
            var num = new double[V][];
            var amp = new double[V];
            for (int v = 0; v < V; ++v)
            {
                noteScratch[v] = new List<VoiceNote>();
                P[v] = new double[half + 1];
                num[v] = new double[half + 1];
            }

            for (int f = 0; f < frameCount; ++f)
            {
                if ((f & 63) == 0)
                    ct.ThrowIfCancellationRequested();

                int start = f * Hop;
                double tMid = (start + FftSize / 2.0) / sampleRate;

                activeVoices.Clear();
                for (int v = 0; v < V; ++v)
                {
                    var ns = noteScratch[v];
                    ns.Clear();
                    foreach (var note in voices[v])
                        if (note.StartSec <= tMid && tMid < note.EndSec + ReleaseSec)
                            ns.Add(note);
                    if (ns.Count > 0)
                        activeVoices.Add(v);
                }
                if (activeVoices.Count == 0)
                    continue;

                // Forward STFT of the windowed frame (NAudio scales the forward transform by 1/N).
                for (int i = 0; i < FftSize; ++i)
                {
                    fft[i].X = (float)(samples[start + i] * w[i]);
                    fft[i].Y = 0f;
                }
                FastFourierTransform.FFT(true, FftBits, fft);

                if (activeVoices.Count == 1)
                {
                    // Only one voice sounds: it owns the whole frame (mask = 1). Inverse fft in place.
                    int v = activeVoices[0];
                    FastFourierTransform.FFT(false, FftBits, fft);
                    var buf = voiceBuf[v];
                    for (int i = 0; i < FftSize; ++i)
                        buf[start + i] += (float)(fft[i].X * w[i]);
                }
                else
                {
                    for (int i = 0; i <= half; ++i)
                        mag[i] = Math.Sqrt((double)fft[i].X * fft[i].X + (double)fft[i].Y * fft[i].Y);

                    // Predicted harmonic spectrum + amplitude per active voice.
                    foreach (int v in activeVoices)
                    {
                        var Pv = P[v];
                        Array.Clear(Pv, 0, half + 1);
                        foreach (var note in noteScratch[v])
                        {
                            double f0 = 440.0 * Math.Pow(2, (note.Pitch - 69) / 12.0 + tuningCents / 1200.0);
                            double velGain = Math.Max(0.0, note.Velocity) / 127.0;
                            for (int h = 1; h <= MaxHarmonics; ++h)
                            {
                                double fh = f0 * h;
                                if (fh >= nyquist)
                                    break;
                                AddHarmonic(Pv, binHz, half, fh, H[h - 1] * velGain);
                            }
                        }
                        double sumP = 0;
                        for (int i = 0; i <= half; ++i)
                            sumP += Pv[i];
                        double a = 0;
                        if (sumP > 0)
                            for (int i = 0; i <= half; ++i)
                                a += Pv[i] / sumP * mag[i];
                        amp[v] = a;
                    }

                    // Soft masks: numerator_v = (amp_v * P_v)^power; mask_v = numerator_v / Σ.
                    Array.Clear(denom, 0, half + 1);
                    foreach (int v in activeVoices)
                    {
                        var Pv = P[v];
                        var nv = num[v];
                        double av = amp[v];
                        for (int i = 0; i <= half; ++i)
                        {
                            double x = av * Pv[i];
                            double numer = x <= 0 ? 0 : Math.Pow(x, MaskPower);
                            nv[i] = numer;
                            denom[i] += numer;
                        }
                    }

                    foreach (int v in activeVoices)
                    {
                        var nv = num[v];
                        for (int i = 0; i <= half; ++i)
                        {
                            double m = nv[i] / (denom[i] + Eps);
                            voiceSpec[i].X = (float)(fft[i].X * m);
                            voiceSpec[i].Y = (float)(fft[i].Y * m);
                        }
                        // Hermitian mirror for the upper half before the inverse transform.
                        for (int i = 1; i < half; ++i)
                        {
                            voiceSpec[FftSize - i].X = voiceSpec[i].X;
                            voiceSpec[FftSize - i].Y = -voiceSpec[i].Y;
                        }
                        FastFourierTransform.FFT(false, FftBits, voiceSpec);
                        var buf = voiceBuf[v];
                        for (int i = 0; i < FftSize; ++i)
                            buf[start + i] += (float)(voiceSpec[i].X * w[i]);
                    }
                }

                // Overlap-add normalisation weight, accumulated once per rendered frame.
                for (int i = 0; i < FftSize; ++i)
                    norm[start + i] += w[i] * w[i];
            }

            for (int n = 0; n < len; ++n)
            {
                double d = norm[n];
                if (d <= Eps)
                    continue;
                double inv = 1.0 / d;
                for (int v = 0; v < V; ++v)
                    voiceBuf[v][n] = (float)(voiceBuf[v][n] * inv);
            }

            return BuildResult(key, voices, voiceBuf, sampleRate, len);
        }

        /// <summary>Adds a Gaussian (in cents) harmonic peak of the given strength around fh into P.</summary>
        static void AddHarmonic(double[] P, double binHz, int half, double fh, double strength)
        {
            if (strength <= 0 || fh <= 0)
                return;
            double loF = fh * Math.Pow(2, -3 * HarmonicSigmaCents / 1200.0);
            double hiF = fh * Math.Pow(2, 3 * HarmonicSigmaCents / 1200.0);
            int lo = Math.Max(1, (int)Math.Floor(loF / binHz));
            int hi = Math.Min(half, (int)Math.Ceiling(hiF / binHz));
            for (int b = lo; b <= hi; ++b)
            {
                double bf = b * binHz;
                double cents = 1200 * Math.Log(bf / fh, 2);
                double z = cents / HarmonicSigmaCents;
                P[b] += strength * Math.Exp(-0.5 * z * z);
            }
        }

        static double EstimateTuning(float[] samples, double[] w, int sampleRate, double binHz, double nyquist,
            List<VoiceNote>[] voices, int frameCount, CancellationToken ct)
        {
            int half = FftSize / 2;
            var fft = new Complex[FftSize];
            var mag = new double[half + 1];
            var active = new List<VoiceNote>(4);

            int steps = (int)(TuningRangeCents / TuningStepCents);
            var score = new double[2 * steps + 1];
            int used = 0;

            int stride = Math.Max(1, frameCount / MaxEstimateFrames);
            for (int f = 0; f < frameCount && used < MaxEstimateFrames; f += stride)
            {
                if ((f & 63) == 0)
                    ct.ThrowIfCancellationRequested();
                int start = f * Hop;
                double tMid = (start + FftSize / 2.0) / sampleRate;
                GatherActive(voices, tMid, active, 3);
                if (active.Count == 0 || active.Count > 2)
                    continue;

                FrameMag(samples, w, start, fft, mag);
                ++used;

                for (int s = 0; s <= 2 * steps; ++s)
                {
                    double cents = (s - steps) * TuningStepCents;
                    double acc = 0;
                    foreach (var note in active)
                    {
                        double f0 = 440.0 * Math.Pow(2, (note.Pitch - 69) / 12.0 + cents / 1200.0);
                        for (int h = 1; h <= MaxHarmonics; ++h)
                        {
                            double fh = f0 * h;
                            if (fh >= nyquist)
                                break;
                            acc += 1.0 / Math.Sqrt(h) * MagNear(mag, binHz, fh);
                        }
                    }
                    score[s] += acc;
                }
            }

            if (used == 0)
                return 0;
            int best = steps; // 0 cents
            for (int s = 0; s <= 2 * steps; ++s)
                if (score[s] > score[best])
                    best = s;
            return (best - steps) * TuningStepCents;
        }

        static double[] EstimateTemplate(float[] samples, double[] w, int sampleRate, double binHz, double nyquist,
            List<VoiceNote>[] voices, int frameCount, double tuningCents, CancellationToken ct)
        {
            int half = FftSize / 2;
            var fft = new Complex[FftSize];
            var mag = new double[half + 1];
            var active = new List<VoiceNote>(2);
            var ratios = new List<double>[MaxHarmonics];
            for (int h = 0; h < MaxHarmonics; ++h)
                ratios[h] = new List<double>();

            int used = 0;
            int stride = Math.Max(1, frameCount / MaxEstimateFrames);
            for (int f = 0; f < frameCount && used < MaxEstimateFrames; f += stride)
            {
                if ((f & 63) == 0)
                    ct.ThrowIfCancellationRequested();
                int start = f * Hop;
                double tMid = (start + FftSize / 2.0) / sampleRate;
                GatherActive(voices, tMid, active, 2);
                if (active.Count != 1)
                    continue;

                FrameMag(samples, w, start, fft, mag);
                var note = active[0];
                double f0 = 440.0 * Math.Pow(2, (note.Pitch - 69) / 12.0 + tuningCents / 1200.0);
                double m1 = MagNear(mag, binHz, f0);
                if (m1 <= Eps)
                    continue;
                ++used;
                for (int h = 1; h <= MaxHarmonics; ++h)
                {
                    double fh = f0 * h;
                    if (fh >= nyquist)
                        break;
                    ratios[h - 1].Add(MagNear(mag, binHz, fh) / m1);
                }
            }

            var H = new double[MaxHarmonics];
            if (used < 20)
            {
                for (int h = 1; h <= MaxHarmonics; ++h)
                    H[h - 1] = 1.0 / h; // fallback
                return H;
            }
            for (int h = 0; h < MaxHarmonics; ++h)
                H[h] = Median(ratios[h]);
            if (H[0] <= 0)
                H[0] = 1;
            return H;
        }

        static void GatherActive(List<VoiceNote>[] voices, double t, List<VoiceNote> results, int cap)
        {
            results.Clear();
            foreach (var voice in voices)
                foreach (var note in voice)
                    if (note.StartSec <= t && t < note.EndSec)
                    {
                        results.Add(note);
                        if (results.Count >= cap)
                            return;
                    }
        }

        static void FrameMag(float[] samples, double[] w, int start, Complex[] fft, double[] mag)
        {
            int half = FftSize / 2;
            for (int i = 0; i < FftSize; ++i)
            {
                fft[i].X = (float)(samples[start + i] * w[i]);
                fft[i].Y = 0f;
            }
            FastFourierTransform.FFT(true, FftBits, fft);
            for (int i = 0; i <= half; ++i)
                mag[i] = Math.Sqrt((double)fft[i].X * fft[i].X + (double)fft[i].Y * fft[i].Y);
        }

        static double MagNear(double[] mag, double binHz, double freq)
        {
            if (freq <= 0)
                return 0;
            double loF = freq * Math.Pow(2, -HarmonicSigmaCents / 1200.0);
            double hiF = freq * Math.Pow(2, HarmonicSigmaCents / 1200.0);
            int lo = Math.Max(1, (int)Math.Floor(loF / binHz));
            int hi = Math.Min(mag.Length - 1, (int)Math.Ceiling(hiF / binHz));
            double m = 0;
            for (int b = lo; b <= hi; ++b)
                if (mag[b] > m)
                    m = mag[b];
            return m;
        }

        static double Median(List<double> values)
        {
            if (values.Count == 0)
                return 0;
            values.Sort();
            int mid = values.Count / 2;
            return values.Count % 2 == 1 ? values[mid] : (values[mid - 1] + values[mid]) / 2.0;
        }

        static ChannelVoiceSet BuildResult(object key, List<VoiceNote>[] voices, float[][] voiceBuf, int sampleRate, int len)
        {
            var arr = new VoiceAudio[voices.Length];
            for (int v = 0; v < voices.Length; ++v)
            {
                var starts = new List<int>();
                var ends = new List<int>();
                foreach (var note in voices[v]) // added in ascending start order
                {
                    int s = Clamp((int)(note.StartSec * sampleRate), 0, len);
                    int e = Clamp((int)((note.EndSec + ReleaseSec) * sampleRate), 0, len);
                    if (e <= s)
                        continue;
                    if (starts.Count > 0 && s <= ends[ends.Count - 1])
                        ends[ends.Count - 1] = Math.Max(ends[ends.Count - 1], e);
                    else
                    {
                        starts.Add(s);
                        ends.Add(e);
                    }
                }
                arr[v] = new VoiceAudio(voiceBuf[v], starts.ToArray(), ends.ToArray());
            }
            return new ChannelVoiceSet(key, arr);
        }

        static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
