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
    /// Phase 2 of the reference algorithm is included here:
    ///   - NNLS per-voice amplitude estimation.
    ///   - Temporal smoothing of per-voice amplitudes (fast attack / slower release).
    ///   - Per-register (low / mid / high) harmonic templates with pitch interpolation.
    ///   - Residual energy (bins unexplained by the harmonic masks) redistributed among active voices.
    ///   - short[] (Q15) voice-buffer storage to halve memory vs float.
    /// Still deferred (not required for visualization quality at this scale):
    ///   - Global MIDI↔WAV onset alignment; pitch-bend / vibrato tracking; synthetic carrier mode.
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

        // Amplitude temporal smoother (one-pole). Distinct from ReleaseSec (note activity extension).
        const double AttackSmoothSec = 0.010;
        const double ReleaseSmoothSec = 0.050;
        const int NnlsIters = 24;

        // MIDI pitch centers for the three register templates; notes blend between neighbors.
        const int RegisterLowCenter = 36;   // C2
        const int RegisterMidCenter = 60;   // C4
        const int RegisterHighCenter = 84;  // C6
        const int NumRegisters = 3;

        // Cap how many frames feed the (cheap) tuning/template estimates.
        const int MaxEstimateFrames = 400;
        const int MinTemplateFrames = 20;
        const int MinRegisterFrames = 10;

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

            // Float accumulators during overlap-add; quantized to Q15 at the end.
            var voiceBuf = new float[V][];
            for (int v = 0; v < V; ++v)
                voiceBuf[v] = new float[len];

            if (len < FftSize)
                return BuildResult(key, voices, voiceBuf, sampleRate, len);

            int frameCount = (len - FftSize) / Hop + 1;
            var norm = new double[len];

            double tuningCents = EstimateTuning(samples, w, sampleRate, binHz, nyquist, voices, frameCount, ct);
            double[][] Hreg = EstimateRegisterTemplates(samples, w, sampleRate, binHz, nyquist, voices, frameCount, tuningCents, ct);

            double dt = (double)Hop / sampleRate;
            double attackKeep = Math.Exp(-dt / AttackSmoothSec);   // weight on previous when rising
            double releaseKeep = Math.Exp(-dt / ReleaseSmoothSec); // weight on previous when falling

            // Reusable per-frame buffers.
            var fft = new Complex[FftSize];
            var voiceSpec = new Complex[FftSize];
            var mag = new double[half + 1];
            var denom = new double[half + 1];
            var residual = new double[half + 1];
            var nnlsRecon = new double[half + 1];
            var activeVoices = new List<int>(V);
            var noteScratch = new List<VoiceNote>[V];
            var P = new double[V][];
            var num = new double[V][];
            var ampRaw = new double[V];
            var ampSmooth = new double[V]; // persists across frames
            var isActive = new bool[V];
            var Hnote = new double[MaxHarmonics]; // scratch for one note's interpolated template
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
                {
                    // Decay smoothed amplitudes toward zero so a later re-entry uses attack.
                    for (int v = 0; v < V; ++v)
                        ampSmooth[v] *= releaseKeep;
                    continue;
                }

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
                    // Keep smoother state coherent with "full ownership" so a later polyphonic frame
                    // attacks/releases from a sensible previous value.
                    ampSmooth[v] = SmoothAmp(ampSmooth[v], 1.0, attackKeep, releaseKeep);
                    for (int u = 0; u < V; ++u)
                        if (u != v)
                            ampSmooth[u] = SmoothAmp(ampSmooth[u], 0.0, attackKeep, releaseKeep);
                }
                else
                {
                    for (int i = 0; i <= half; ++i)
                        mag[i] = Math.Sqrt((double)fft[i].X * fft[i].X + (double)fft[i].Y * fft[i].Y);

                    // Predicted harmonic spectrum per active voice (sum of its notes, each with a
                    // pitch-interpolated register template).
                    foreach (int v in activeVoices)
                    {
                        var Pv = P[v];
                        Array.Clear(Pv, 0, half + 1);
                        foreach (var note in noteScratch[v])
                        {
                            InterpolateTemplate(Hreg, note.Pitch, Hnote);
                            double f0 = 440.0 * Math.Pow(2, (note.Pitch - 69) / 12.0 + tuningCents / 1200.0);
                            double velGain = Math.Max(0.0, note.Velocity) / 127.0;
                            for (int h = 1; h <= MaxHarmonics; ++h)
                            {
                                double fh = f0 * h;
                                if (fh >= nyquist)
                                    break;
                                AddHarmonic(Pv, binHz, half, fh, Hnote[h - 1] * velGain);
                            }
                        }
                    }

                    // NNLS: mag ≈ Σ_v a_v P_v, a_v ≥ 0.
                    SolveNnls(P, activeVoices, mag, half, ampRaw, nnlsRecon);

                    // Temporal smooth, then scale each template by its smoothed amplitude.
                    Array.Clear(isActive, 0, V);
                    foreach (int v in activeVoices)
                        isActive[v] = true;
                    for (int v = 0; v < V; ++v)
                        ampSmooth[v] = SmoothAmp(ampSmooth[v], isActive[v] ? ampRaw[v] : 0.0, attackKeep, releaseKeep);

                    foreach (int v in activeVoices)
                    {
                        var Pv = P[v];
                        double a = ampSmooth[v];
                        for (int i = 0; i <= half; ++i)
                            Pv[i] *= a;
                    }

                    // Soft masks: numerator_v = P_v^power; mask_v = numerator_v / Σ.
                    Array.Clear(denom, 0, half + 1);
                    foreach (int v in activeVoices)
                    {
                        var Pv = P[v];
                        var nv = num[v];
                        for (int i = 0; i <= half; ++i)
                        {
                            double x = Pv[i];
                            double numer = x <= 0 ? 0 : Math.Pow(x, MaskPower);
                            nv[i] = numer;
                            denom[i] += numer;
                        }
                    }

                    // Residual: bins the harmonic model left unexplained. Distribute among active
                    // voices in proportion to their smoothed amplitudes so the sum still ≈ X.
                    double ampSum = 0;
                    foreach (int v in activeVoices)
                        ampSum += Math.Max(ampSmooth[v], 0);
                    if (ampSum <= Eps)
                        ampSum = activeVoices.Count; // equal share fallback

                    for (int i = 0; i <= half; ++i)
                    {
                        double explained = 0;
                        foreach (int v in activeVoices)
                            explained += num[v][i] / (denom[i] + Eps);
                        residual[i] = Math.Max(0.0, 1.0 - explained);
                    }

                    foreach (int v in activeVoices)
                    {
                        var nv = num[v];
                        double share = Math.Max(ampSmooth[v], 0) / ampSum;
                        for (int i = 0; i <= half; ++i)
                        {
                            double m = nv[i] / (denom[i] + Eps) + residual[i] * share;
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

        static double SmoothAmp(double prev, double target, double attackKeep, double releaseKeep)
        {
            double keep = target > prev ? attackKeep : releaseKeep;
            return keep * prev + (1.0 - keep) * target;
        }

        /// <summary>
        /// Multiplicative-update NNLS for mag ≈ Σ a_v P_v with a ≥ 0. V ≤ 4 so a few dozen
        /// iterations are cheap; near-zero columns stay near zero via the Eps ridge.
        /// </summary>
        static void SolveNnls(double[][] P, List<int> active, double[] mag, int half, double[] ampOut, double[] recon)
        {
            int n = active.Count;
            // Seed from a simple weighted-energy estimate (same as the phase-1 fallback).
            for (int i = 0; i < n; ++i)
            {
                int v = active[i];
                var Pv = P[v];
                double sumP = 0, a = 0;
                for (int b = 0; b <= half; ++b)
                    sumP += Pv[b];
                if (sumP > Eps)
                    for (int b = 0; b <= half; ++b)
                        a += Pv[b] / sumP * mag[b];
                ampOut[v] = Math.Max(a, Eps);
            }

            // a_v ← a_v * (P_v · mag) / (P_v · (Σ_u a_u P_u) + eps)
            for (int iter = 0; iter < NnlsIters; ++iter)
            {
                Array.Clear(recon, 0, half + 1);
                for (int i = 0; i < n; ++i)
                {
                    int v = active[i];
                    var Pv = P[v];
                    double a = ampOut[v];
                    for (int b = 0; b <= half; ++b)
                        recon[b] += a * Pv[b];
                }
                for (int i = 0; i < n; ++i)
                {
                    int v = active[i];
                    var Pv = P[v];
                    double num = 0, den = 0;
                    for (int b = 0; b <= half; ++b)
                    {
                        double p = Pv[b];
                        num += p * mag[b];
                        den += p * recon[b];
                    }
                    ampOut[v] *= num / (den + Eps);
                    if (ampOut[v] < 0)
                        ampOut[v] = 0;
                }
            }
        }

        /// <summary>Writes the pitch-interpolated harmonic template for <paramref name="pitch"/> into Hout.</summary>
        static void InterpolateTemplate(double[][] Hreg, int pitch, double[] Hout)
        {
            // Piecewise-linear blend across the three register centers.
            int r0, r1;
            double t;
            if (pitch <= RegisterLowCenter)
            {
                r0 = r1 = 0; t = 0;
            }
            else if (pitch >= RegisterHighCenter)
            {
                r0 = r1 = 2; t = 0;
            }
            else if (pitch <= RegisterMidCenter)
            {
                r0 = 0; r1 = 1;
                t = (pitch - RegisterLowCenter) / (double)(RegisterMidCenter - RegisterLowCenter);
            }
            else
            {
                r0 = 1; r1 = 2;
                t = (pitch - RegisterMidCenter) / (double)(RegisterHighCenter - RegisterMidCenter);
            }
            var A = Hreg[r0];
            var B = Hreg[r1];
            for (int h = 0; h < MaxHarmonics; ++h)
                Hout[h] = A[h] * (1.0 - t) + B[h] * t;
        }

        static int RegisterIndex(int pitch)
        {
            // Nearest of the three centers for template estimation binning.
            int dLow = Math.Abs(pitch - RegisterLowCenter);
            int dMid = Math.Abs(pitch - RegisterMidCenter);
            int dHigh = Math.Abs(pitch - RegisterHighCenter);
            if (dLow <= dMid && dLow <= dHigh) return 0;
            if (dMid <= dHigh) return 1;
            return 2;
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

        /// <summary>
        /// Learns one harmonic template per pitch register from single-note frames. Thin registers
        /// fall back to the global median (or 1/h if even that is thin).
        /// </summary>
        static double[][] EstimateRegisterTemplates(float[] samples, double[] w, int sampleRate, double binHz,
            double nyquist, List<VoiceNote>[] voices, int frameCount, double tuningCents, CancellationToken ct)
        {
            int half = FftSize / 2;
            var fft = new Complex[FftSize];
            var mag = new double[half + 1];
            var active = new List<VoiceNote>(2);

            var ratios = new List<double>[NumRegisters][];
            var usedPerReg = new int[NumRegisters];
            for (int r = 0; r < NumRegisters; ++r)
            {
                ratios[r] = new List<double>[MaxHarmonics];
                for (int h = 0; h < MaxHarmonics; ++h)
                    ratios[r][h] = new List<double>();
            }
            var globalRatios = new List<double>[MaxHarmonics];
            for (int h = 0; h < MaxHarmonics; ++h)
                globalRatios[h] = new List<double>();
            int usedGlobal = 0;

            int stride = Math.Max(1, frameCount / MaxEstimateFrames);
            for (int f = 0; f < frameCount && usedGlobal < MaxEstimateFrames; f += stride)
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

                int reg = RegisterIndex(note.Pitch);
                ++usedPerReg[reg];
                ++usedGlobal;
                for (int h = 1; h <= MaxHarmonics; ++h)
                {
                    double fh = f0 * h;
                    if (fh >= nyquist)
                        break;
                    double ratio = MagNear(mag, binHz, fh) / m1;
                    ratios[reg][h - 1].Add(ratio);
                    globalRatios[h - 1].Add(ratio);
                }
            }

            var fallback = new double[MaxHarmonics];
            if (usedGlobal < MinTemplateFrames)
            {
                for (int h = 1; h <= MaxHarmonics; ++h)
                    fallback[h - 1] = 1.0 / h;
            }
            else
            {
                for (int h = 0; h < MaxHarmonics; ++h)
                    fallback[h] = Median(globalRatios[h]);
                if (fallback[0] <= 0)
                    fallback[0] = 1;
            }

            var Hreg = new double[NumRegisters][];
            for (int r = 0; r < NumRegisters; ++r)
            {
                Hreg[r] = new double[MaxHarmonics];
                if (usedPerReg[r] < MinRegisterFrames)
                {
                    Array.Copy(fallback, Hreg[r], MaxHarmonics);
                    continue;
                }
                for (int h = 0; h < MaxHarmonics; ++h)
                    Hreg[r][h] = Median(ratios[r][h]);
                if (Hreg[r][0] <= 0)
                    Hreg[r][0] = 1;
            }
            return Hreg;
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
                arr[v] = new VoiceAudio(ToQ15(voiceBuf[v]), starts.ToArray(), ends.ToArray());
            }
            return new ChannelVoiceSet(key, arr);
        }

        /// <summary>Quantize a float −1..1 buffer to Q15 short (clamped).</summary>
        static short[] ToQ15(float[] src)
        {
            var dst = new short[src.Length];
            for (int i = 0; i < src.Length; ++i)
            {
                float x = src[i];
                if (x > 1f) x = 1f;
                else if (x < -1f) x = -1f;
                dst[i] = (short)(x * 32767f);
            }
            return dst;
        }

        static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
    }
}
