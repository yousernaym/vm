using LibSidWiz;
using LibSidWiz.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualMusic
{
    public class WaveformPanel
    {
        private Texture2D _waveTex;
        private Rectangle _overlayRect;
        private WaveformRenderer _renderer;
        private Channel[] _channels;

        // Reusable CPU buffer when copying from Bitmap to Texture2D
        private byte[] _framePixels;

        // Target UI refresh rate for libSidWiz frame math. 60 is a good default.
        private const int UiFps = 60;

        GraphicsDevice _gfxDevice;
        SpriteBatch _spriteBatch;

        internal async void Init(GraphicsDevice gfxDevice, SpriteBatch spriteBatch)
        {
            _gfxDevice  = gfxDevice;
            _spriteBatch = spriteBatch;

            Resize();

            _renderer = new WaveformRenderer
            {
                Width = _overlayRect.Width,
                Height = _overlayRect.Height,
                Columns = 1, // stack channels vertically
                RenderingBounds = new System.Drawing.Rectangle(0, 0, _overlayRect.Width, _overlayRect.Height),
                BackgroundColor = System.Drawing.Color.Transparent, // transparent so it blends over your 3D scene
                FramesPerSecond = UiFps
            }; // Columns + RenderingBounds are used to compute per-channel boxes. :contentReference[oaicite:0]{index=0}

            // --- Load one or more WAV files into Channels ---
            // Replace these with your real paths:
            var wavs = new[]
            {
                "Content/test.wav",
                "Content/test2.wav"
        };

            _channels = new Channel[wavs.Length];
            for (int i = 0; i < wavs.Length; i++)
            {
                var ch = new Channel(autoReloadOnSettingChanged: false)
                {
                    Algorithm = new PeakSpeedTrigger(),
                    Filename = wavs[i],
                    Side = Channel.Sides.Mix,     // or Left/Right if you want a specific side
                    HighPassFilter = false,       // optional
                    LineColor = System.Drawing.Color.Lime,
                    LineWidth = 2f,
                    ZeroLineColor = System.Drawing.Color.FromArgb(60, 255, 255, 255),
                    ZeroLineWidth = 1f,
                    SmoothLines = true,
                    RenderIfSilent = true,
                    // How wide the window is in samples (defaults to 1500). You can also set ViewWidthInMilliseconds.
                    ViewWidthInSamples = 1500
                };

                // This reads and analyzes the wav via SampleBuffer (NAudio under the hood),
                // sets SampleRate/Length/Max/SampleCount, and prepares trigger data. :contentReference[oaicite:1]{index=1}
                await ch.LoadDataAsync();

                _renderer.AddChannel(ch); // keep adding channels to stack vertically (Columns=1). :contentReference[oaicite:2]{index=2}
                _channels[i] = ch;
            }


            // libSidWiz needs a single SamplingRate for its internal frame/sample math.
            // Use the first channel’s rate (ideally all WAVs match rates). :contentReference[oaicite:3]{index=3}
            _renderer.SamplingRate = _channels[0].SampleRate; // set once after loading. :contentReference[oaicite:4]{index=4}

            // Create the GPU texture for the overlay
            _waveTex = new Texture2D(gfxDevice, _renderer.Width, _renderer.Height, false, SurfaceFormat.Color);

            // Reuse a CPU buffer for pixel copy each frame
            _framePixels = new byte[_renderer.Width * _renderer.Height * 4];
        }

        private void Resize()
        {
            var vp = _gfxDevice.Viewport;
            
            _overlayRect = new Rectangle(0, 0, (int)(vp.Width * 0.25f), vp.Height); // leftmost 25%
            if (_renderer != null)
            {
                _renderer.Width = _overlayRect.Width;
                _renderer.Height = _overlayRect.Height;
                _renderer.RenderingBounds = new System.Drawing.Rectangle(0, 0, _overlayRect.Width, _overlayRect.Height); // local in overlay. :contentReference[oaicite:9]{index=9}

                // Recreate texture if size changes
                _waveTex?.Dispose();
                _waveTex = new Texture2D(_gfxDevice, _renderer.Width, _renderer.Height, false, SurfaceFormat.Color);
                _framePixels = new byte[_renderer.Width * _renderer.Height * 4];
            }
        }

        internal void Draw(double songPosS)
        {
            if (_waveTex == null)
                return;
            var pixels = _renderer.RenderFrame(songPosS);
            _waveTex.SetData(pixels);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            _spriteBatch.Draw(_waveTex, _overlayRect, Microsoft.Xna.Framework.Color.White);
            _spriteBatch.End();
        }

        static void BitmapToTexture2D(System.Drawing.Bitmap bitmap, Texture2D texture)
        {
            // Lock the bitmap's bits
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                // Copy bitmap data to a byte array
                int bytes = bitmapData.Stride * bitmap.Height;
                byte[] pixelData = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixelData, 0, bytes);

                // MonoGame expects Color format (RGBA), but System.Drawing uses BGRA
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    byte b = pixelData[i];
                    byte r = pixelData[i + 2];
                    pixelData[i] = r;
                    pixelData[i + 2] = b;
                }

                // Set data to Texture2D
                texture.SetData(pixelData);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
