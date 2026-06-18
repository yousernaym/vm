using Microsoft.Xna.Framework.Graphics;
using System;

namespace VisualMusic
{
    /// <summary>
    /// Abstracts the drawing surface used by Project.drawSong().
    /// </summary>
    public interface ISongDrawHost
    {
        // ---- Frame setup ----
        void DrawBackground();
        void InitFrame();

        /// <summary>
        /// Sets up a crossfade between two background images for keyframe-driven transitions.
        /// <paramref name="pathA"/> is the "from" image, <paramref name="pathB"/> is the "to" image
        /// (null or empty = no texture on that side), and <paramref name="blend"/> ∈ [0,1] is the
        /// mix amount (0 = fully A, 1 = fully B).  Called every frame from
        /// <see cref="Project.InterpolatePropertyKeyframes"/>.
        /// </summary>
        void SetBackgroundCrossfade(string pathA, string pathB, float blend);

        // ---- Graphics resources ----
        GraphicsDevice GraphicsDevice { get; }
        bool ForceDefaultNoteStyle { get; }
        SpriteFont LyricsFont { get; }
        SpriteBatch SpriteBatch { get; }
        WaveformPanel WaveformPanel { get; }

        // ---- Viewport size ----
        int ClientWidth { get; }
        int ClientHeight { get; }

        // ---- Input state ----
        bool LeftMbPressed { get; }
        bool RightMbPressed { get; }

        // ---- Timing (for playback sync) ----
        TimeSpan TotalTimeElapsed { get; }

        // ---- Notifications ----
        /// <summary>Request a repaint / signal that the song position changed.</summary>
        void Invalidate();
        /// <summary>Called whenever NormSongPos changes (e.g. to update UI controls).</summary>
        void NotifySongPosChanged();
    }
}
