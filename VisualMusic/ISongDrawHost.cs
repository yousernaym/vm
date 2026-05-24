using Microsoft.Xna.Framework.Graphics;
using System;

namespace VisualMusic
{
    /// <summary>
    /// Abstracts the drawing surface used by Project.drawSong(), so the same
    /// drawing code works with both the WinForms SongPanel and the WPF SongRenderer.
    /// </summary>
    public interface ISongDrawHost
    {
        // ---- Frame setup ----
        void DrawBackground();
        void InitFrame();

        // ---- Graphics resources ----
        GraphicsDevice GraphicsDevice { get; }
        bool ForceDefaultNoteStyle { get; }
        SpriteFont LyricsFont { get; }
        SpriteBatch SpriteBatch { get; }
        WaveformPanel WaveformPanel { get; }

        // ---- Viewport size ----
        int ClientWidth  { get; }
        int ClientHeight { get; }

        // ---- Timing (for playback sync) ----
        TimeSpan TotalTimeElapsed { get; }

        // ---- Notifications ----
        /// <summary>Request a repaint / signal that the song position changed.</summary>
        void Invalidate();
        /// <summary>Called whenever NormSongPos changes (e.g. to update UI controls).</summary>
        void NotifySongPosChanged();
    }
}
