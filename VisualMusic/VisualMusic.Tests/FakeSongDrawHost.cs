using Microsoft.Xna.Framework.Graphics;
using System;

namespace VisualMusic.Tests
{
    /// <summary>Minimal <see cref="ISongDrawHost"/> for Project tempo/position unit tests (no GPU).</summary>
    sealed class FakeSongDrawHost : ISongDrawHost
    {
        public void DrawBackground() { }
        public void InitFrame() { }
        public void SetBackgroundCrossfade(string pathA, string pathB, float blend) { }
        public GraphicsDevice GraphicsDevice => null;
        public bool ForceDefaultNoteStyle => false;
        public SpriteFont LyricsFont => null;
        public SpriteBatch SpriteBatch => null;
        public WaveformPanel WaveformPanel => null;
        public int ClientWidth => 1280;
        public int ClientHeight => 720;
        public bool LeftMbPressed => false;
        public bool RightMbPressed => false;
        public TimeSpan TotalTimeElapsed { get; set; } = TimeSpan.Zero;
        public void Invalidate() { }
        public void NotifySongPosChanged() { }
    }
}
