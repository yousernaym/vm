using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using WinFormsGraphicsDevice;
using WinKeys = System.Windows.Forms.Keys;

namespace VisualMusic
{
    using RectangleF = System.Drawing.RectangleF;

    /// <summary>
    /// WPF-compatible MonoGame renderer extracted from SongPanel.
    /// Has no dependency on WinForms Control or GraphicsDeviceControl.
    /// Hosted by MonoGameHost (HwndHost).
    /// </summary>
    public class SongRenderer : ISongDrawHost
    {
        // --- Core rendering ---
        GraphicsDevice graphicsDevice;
        Action<int, int> resetDevice;
        ServiceContainer services;
        ContentManager content;
        SpriteBatch spriteBatch;
        BlendState blendState;
        RasterizerState rastState;
        Effect postProcessFx;
        ScreenQuad quad;
        Texture2D regionSelectTexture;
        Texture2D backgroundTexture;
        readonly object renderLock = new object();

        // --- Layout ---
        int clientWidth = 1;
        int clientHeight = 1;

        // --- Timing ---
        Stopwatch stopwatch = new Stopwatch();
        TimeSpan oldTime = new TimeSpan(0);
        double deltaTimeS;
        bool isRenderingVideo = false;
        const int CmFaceSide = 4096;

        // --- Input state ---
        bool leftMbPressed;
        bool rightMbPressed;
        bool selectingRegion = false;
        bool mergeRegionSelection = false;
        bool mousePosScrollSong = false;
        bool isPausingWhileScrolling = false;
        double scrollCenter = 0;
        Rectangle selectedScreenRegion;
        bool forceDefaultNoteStyle = false;

        // --- Dependencies ---
        ITrackSelectionService trackSelection;

        // --- Callbacks ---
        /// <summary>Sets the screen-space cursor position (x,y in host client coordinates).</summary>
        public Action<int, int> SetCursorPosition { get; set; }

        // --- Public properties ---
        public SpriteFont LyricsFont { get; private set; }
        Project _project;
        public Project Project
        {
            get => _project;
            set
            {
                _project = value;
                NoteStyle.SetProject(value);
                Project.SetDrawHost(value != null ? this : null);
            }
        }
        public float NormMouseX { get; set; }
        public float NormMouseY { get; set; }
        public bool LeftMbPressed => leftMbPressed;
        public bool RightMbPressed => rightMbPressed;
        public bool SelectingRegion => selectingRegion;
        public SpriteBatch SpriteBatch => spriteBatch;
        public GraphicsDevice GraphicsDevice => graphicsDevice;
        public ContentManager Content => content;
        public WaveformPanel WaveformPanel { get; private set; } = new WaveformPanel();
        public TimeSpan TotalTimeElapsed => stopwatch.Elapsed;

        public bool ForceDefaultNoteStyle
        {
            get => forceDefaultNoteStyle;
            set => forceDefaultNoteStyle = value;
        }

        public const float SmallScrollStep = 1.0f / 16;
        public const float LargeScrollStep = 1.0f;

        public delegate void Delegate_songPosChanged();
        public Delegate_songPosChanged OnSongPosChanged { get; set; }

        public void SetTrackSelectionService(ITrackSelectionService service) => trackSelection = service;

        public void Initialize(GraphicsDevice gd, Action<int, int> resetDeviceCallback, ServiceContainer svc, int width, int height)
        {
            graphicsDevice = gd;
            resetDevice = resetDeviceCallback;
            services = svc;
            clientWidth = Math.Max(1, width);
            clientHeight = Math.Max(1, height);

            stopwatch.Start();
            spriteBatch = new SpriteBatch(graphicsDevice);
            blendState = BlendState.AlphaBlend;
            rastState = new RasterizerState()
            {
                MultiSampleAntiAlias = true,
                CullMode = CullMode.None
            };

            content = new ContentManager(services, "Content");
            NoteStyle.SetGraphicsDevice(graphicsDevice);
            NoteStyle.SetContent(content);
            NoteStyle.sInitAllStyles();
            LyricsFont = content.Load<SpriteFont>("Font");

            regionSelectTexture = new Texture2D(graphicsDevice, 1, 1);
            regionSelectTexture.SetData(new[] { Color.White });

            quad = new ScreenQuad(graphicsDevice);
            postProcessFx = content.Load<Effect>("PostProcess");
            postProcessFx.CurrentTechnique = postProcessFx.Techniques["Technique1"];

            WaveformPanel.Init(graphicsDevice, spriteBatch);
        }

        public void Update(double dt)
        {
            if (Project?.Notes == null || isRenderingVideo)
                return;

            TimeSpan newTime = stopwatch.Elapsed;
            deltaTimeS = (newTime - oldTime).TotalSeconds;
            oldTime = newTime;

            selectRegion();
            Project.update(deltaTimeS);
            scrollSong();
        }

        // ---- Drawing ----

        public string BeginDraw()
        {
            if (graphicsDevice == null)
                return "No graphics device";

            string err = handleDeviceReset();
            if (!string.IsNullOrEmpty(err))
                return err;

            Viewport vp = new Viewport
            {
                X = 0,
                Y = 0,
                Width = graphicsDevice.PresentationParameters.BackBufferWidth,
                Height = graphicsDevice.PresentationParameters.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            };
            graphicsDevice.Viewport = vp;
            return null;
        }

        public void Draw()
        {
            if (graphicsDevice == null) return;
            graphicsDevice.BlendState = blendState;
            graphicsDevice.Clear(Color.Black);
            if (Project == null) return;

            Project.drawSong();

            if (selectingRegion && selectedScreenRegion.Width != 0 && selectedScreenRegion.Height != 0)
            {
                spriteBatch.Begin();
                Rectangle normRect = normalizeRect(selectedScreenRegion);
                spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, normRect.Width, 1), Color.White);
                spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, 1, normRect.Height), Color.White);
                spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Bottom, normRect.Width, 1), Color.White);
                spriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Right, normRect.Top, 1, normRect.Height), Color.White);
                spriteBatch.End();
            }
        }

        public void EndDraw()
        {
            try { graphicsDevice.Present(); }
            catch { }
        }

        string handleDeviceReset()
        {
            switch (graphicsDevice.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Lost:
                    return "Graphics device lost";
                case GraphicsDeviceStatus.NotReset:
                    try { resetDevice?.Invoke(clientWidth, clientHeight); }
                    catch (Exception e) { return "Graphics device reset failed\n\n" + e; }
                    break;
                default:
                    var pp = graphicsDevice.PresentationParameters;
                    if (clientWidth != pp.BackBufferWidth || clientHeight != pp.BackBufferHeight)
                    {
                        try { resetDevice?.Invoke(clientWidth, clientHeight); }
                        catch (Exception e) { return "Graphics device reset failed\n\n" + e; }
                    }
                    break;
            }
            return null;
        }

        public void OnResize(int width, int height)
        {
            clientWidth = Math.Max(1, width);
            clientHeight = Math.Max(1, height);
        }

        // ---- Input ----

        public void HandleMouseDown(bool isLeft, bool isShiftHeld, int x, int y)
        {
            if (Project?.Notes == null)
                return;

            if (isLeft)
            {
                leftMbPressed = true;
                if (isShiftHeld)
                    mergeRegionSelection = true;
                selectedScreenRegion.X = (int)((NormMouseX * 0.5f + 0.5f) * clientWidth);
                selectedScreenRegion.Y = (int)(NormMouseY * clientHeight);
            }
            else
            {
                rightMbPressed = true;
                mousePosScrollSong = true;
                scrollCenter = NormMouseX;
                if (Project.IsPlaying)
                {
                    isPausingWhileScrolling = true;
                    Project.togglePlayback();
                }
            }
        }

        public void HandleMouseUp(bool isLeft)
        {
            if (isLeft)
            {
                leftMbPressed = false;
                mergeRegionSelection = false;
            }
            else
            {
                rightMbPressed = false;
                mousePosScrollSong = false;
                if (isPausingWhileScrolling && Project != null && !Project.IsPlaying)
                {
                    Project.togglePlayback();
                    isPausingWhileScrolling = false;
                }
            }
        }

        public void HandleMouseMove(int x, int y, int workAreaHeight)
        {
            int middleX = clientWidth / 2;
            int middleY = clientHeight / 2;

            NormMouseX = (float)(x - middleX) * 2 / clientWidth;
            NormMouseY = (float)y / clientHeight;

            if (Camera.MouseRot)
            {
                NormMouseX = (float)(x - middleX) * 2 / workAreaHeight;
                NormMouseY = (float)(y - middleY) * 2 / workAreaHeight;
                SetCursorPosition?.Invoke(middleX, middleY);
                Project?.getKeyFrameAtSongPos()?.ProjProps.Camera.ApplyMouseRot(NormMouseX, NormMouseY);
            }
        }

        /// <summary>Returns true if the key was fully handled (suppress further processing).</summary>
        public bool HandleKeyDown(int vkCode)
        {
            var keyFrame = Project?.getKeyFrameAtSongPos();
            if (keyFrame == null) return false;

            bool suppress = false;
            var key = (WinKeys)vkCode;
            var modifiers = System.Windows.Forms.Control.ModifierKeys;

            if (keyFrame.ProjProps.Camera.control(key, true, modifiers))
            {
                suppress = true;
                foreach (var other in Project.KeyFrames.Values)
                {
                    if (keyFrame != other && other.Selected)
                    {
                        other.ProjProps.Camera.Pos = keyFrame.ProjProps.Camera.Pos;
                        other.ProjProps.Camera.Orientation = keyFrame.ProjProps.Camera.Orientation;
                    }
                }
            }

            if (key == WinKeys.Space)
            {
                Project?.togglePlayback();
                suppress = true;
            }

            if (key == WinKeys.Z)
            {
                ForceDefaultNoteStyle = true;
                if (Project != null)
                {
                    for (int t = 1; t < Project.TrackViews.Count; t++)
                    {
                        TrackProps tprops = Project.TrackViews[t].TrackProps;
                        NoteStyleType currentNoteStyle = (NoteStyleType)tprops.StyleProps.Type;
                        if (tprops.ActiveNoteStyle.GetType() != typeof(NoteStyle_Bar))
                        {
                            tprops.StyleProps.Type = NoteStyleType.Bar;
                            Project.TrackViews[t].createOcTree(Project, Project.GlobalTrackProps);
                            tprops.StyleProps.Type = currentNoteStyle;
                        }
                    }
                }
            }

            return suppress;
        }

        // ---- Selection ----

        void selectRegion()
        {
            if (trackSelection == null || trackSelection.TrackListCount == 0)
                return;

            if (leftMbPressed)
            {
                selectingRegion = true;

                Point mousePos = new Point(
                    (int)((NormMouseX * 0.5f + 0.5f) * clientWidth),
                    (int)(NormMouseY * clientHeight));
                selectedScreenRegion.Width = mousePos.X - selectedScreenRegion.X;
                selectedScreenRegion.Height = mousePos.Y - selectedScreenRegion.Y;

                if (selectedScreenRegion.Width == 0 || selectedScreenRegion.Height == 0)
                    return;

                RectangleF normScreenSelection = new RectangleF(
                    (float)selectedScreenRegion.X / clientWidth,
                    (float)selectedScreenRegion.Y / clientHeight,
                    (float)selectedScreenRegion.Width / clientWidth,
                    (float)selectedScreenRegion.Height / clientHeight);
                normScreenSelection.X *= 2; normScreenSelection.Y *= -2;
                normScreenSelection.Width *= 2; normScreenSelection.Height *= -2;
                normScreenSelection.Offset(-1, 1);

                Matrix selectionFrustumMat = Project.Props.Camera.VpMat;
                float scaleX = 2 / Math.Abs(normScreenSelection.Width);
                float scaleY = 2.0f / Math.Abs(normScreenSelection.Height);
                selectionFrustumMat *= Matrix.CreateScale(scaleX, scaleY, 1);
                Vector2 normCenter = new Vector2(
                    normScreenSelection.X + normScreenSelection.Width / 2,
                    normScreenSelection.Y + normScreenSelection.Height / 2);
                selectionFrustumMat *= Matrix.CreateTranslation(-normCenter.X * scaleX, -normCenter.Y * scaleY, 0);
                BoundingFrustum selectionFrustum = new BoundingFrustum(selectionFrustumMat);

                int selectedCount = 0;
                for (int i = 1; i < Project.TrackViews.Count; i++)
                {
                    if (Project.TrackViews[i].MidiTrack.Notes.Count > 0 &&
                        Project.TrackViews[i].OcTree.areObjectsInFrustum(selectionFrustum, Project.SongPosP - Project.PlaybackOffsetP, Project, Project.TrackViews[i].TrackProps))
                    {
                        trackSelection.SetTrackSelected(i, true);
                        selectedCount++;
                    }
                    else if (!mergeRegionSelection && trackSelection.TrackListCount > 1)
                        trackSelection.SetTrackSelected(i, false);
                }

                if (selectedCount == 0 && !mergeRegionSelection)
                    trackSelection.SetTrackSelected(0, true);
                else if (selectedCount > 0)
                    trackSelection.SetTrackSelected(0, false);
            }
            else if (selectingRegion)
            {
                selectingRegion = false;
            }
        }

        Rectangle normalizeRect(Rectangle r)
        {
            if (r.Height < 0) { r.Y += r.Height; r.Height = -r.Height; }
            if (r.Width < 0) { r.X += r.Width; r.Width = -r.Width; }
            return r;
        }

        // ---- Scroll ----

        public void scrollSong()
        {
            if (mousePosScrollSong && !selectingRegion && Project != null)
            {
                double dNormMouseX = (double)NormMouseX - scrollCenter;
                Project.NormSongPos += (float)(Math.Pow(dNormMouseX, 2) * Math.Sign(dNormMouseX) * deltaTimeS * 0.3f);
            }
        }

        // ---- Background image ----

        public void LoadBackgroundImage(string path)
        {
            UnloadBackgroundImage();
            if (!string.IsNullOrWhiteSpace(path))
            {
                try { backgroundTexture = Texture2D.FromFile(graphicsDevice, path); }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Could not load image \"{Path.GetFileName(path)}\". {ex.Message}",
                        "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
        }

        public void UnloadBackgroundImage()
        {
            backgroundTexture?.Dispose();
            backgroundTexture = null;
        }

        public void DrawBackground()
        {
            postProcessFx.Parameters["saturationLevel"].SetValue(Project.Props.BackgroundImageSaturation);
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, postProcessFx, null);
            if (backgroundTexture != null)
                spriteBatch.Draw(backgroundTexture,
                    new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                    new Color(Project.Props.BackgroundImageOpacity, Project.Props.BackgroundImageOpacity, Project.Props.BackgroundImageOpacity));
            spriteBatch.End();
        }

        public void InitFrame()
        {
            graphicsDevice.RasterizerState = rastState;
        }

        // ---- ISongDrawHost extras ----
        public int ClientWidth  => clientWidth;
        public int ClientHeight => clientHeight;
        // TotalTimeElapsed already declared above (line ~86).
        /// <summary>No-op: WPF render loop is continuous — no explicit invalidation needed.</summary>
        public void Invalidate() { }
        /// <summary>No-op: WPF uses data binding / polling for position display.</summary>
        public void NotifySongPosChanged() { }

        public void updateTimeStamp()
        {
            oldTime = stopwatch.Elapsed;
        }

        // ---- Video rendering ----

        public void renderVideo(string videoFilePath, IRenderProgressCallback progress, VideoExportOptions options)
        {
            lock (renderLock)
            {
                VideoFormat videoFormat = new VideoFormat((uint)options.Width, (uint)options.Height);
                videoFormat.fps = options.Fps;
                if (!Media.beginVideoEnc(videoFilePath, Project.AudioFilePath, videoFormat,
                    Project.Props.AudioOffset + Project.Props.PlaybackOffsetS,
                    options.Sphere && options.SphericalMetadata, options.SphericalStereo,
                    options.VideoCodec, options.VideoCrf))
                {
                    lock (progress.CancelLock) { }
                    progress.ShowMessage("Couldn't initialize video encoding.");
                    return;
                }

                isRenderingVideo = true;
                RenderTarget2D[] rt32 = new RenderTarget2D[2];
                RenderTargetCube rtCube = null;
                RenderTarget2D rt8 = null;
                RenderTarget2D rtFinal = null;
                Effect cubeToPlaneFx = null;
                Effect ssFx = null;
                uint[] frameData = null;
                Project backup = Project;
                Project = Project.clone();

                try
                {
                    try
                    {
                        if (options.Sphere)
                        {
                            rtCube = new RenderTargetCube(graphicsDevice, CmFaceSide, true,
                                SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
                            cubeToPlaneFx = content.Load<Effect>("CubeToPlane");
                            cubeToPlaneFx.Parameters["CubeMap"].SetValue(rtCube);
                            cubeToPlaneFx.Parameters["FrameSamples"].SetValue((float)1);
                        }
                        rt8 = new RenderTarget2D(graphicsDevice, options.SSAAWidth, options.SSAAHeight,
                            options.SSAAEnabled, SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
                        rtFinal = options.SSAAEnabled
                            ? new RenderTarget2D(graphicsDevice, options.Width, options.Height, false,
                                SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents)
                            : rt8;
                        frameData = new uint[options.Width * options.Height];
                    }
                    catch (Exception e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Message);
                        lock (progress.CancelLock) { }
                        return;
                    }

                    ssFx = content.Load<Effect>("ss");
                    Camera.InvertY = options.Sphere;
                    Project.setSongPosS(0, false);

                    while (Project.NormSongPos < 1 && !progress.Cancel)
                    {
                        Project.interpolateFrames();
                        drawVideoFrame(0, videoFormat.fps, 1, options, rtCube, rt32, rt8, cubeToPlaneFx);
                        if (options.SSAAEnabled)
                        {
                            graphicsDevice.SetRenderTarget(rtFinal);
                            graphicsDevice.Clear(Color.Transparent);
                            ssFx.Parameters["FrameTex"].SetValue(rt8);
                            ssFx.CurrentTechnique.Passes[0].Apply();
                            quad.draw();
                        }
                        graphicsDevice.SetRenderTarget(null);
                        rtFinal.GetData<uint>(frameData);
                        if (!Media.writeFrame(frameData))
                        {
                            progress.ShowMessage("Couldn't add frame");
                            break;
                        }
                        double frameTimeS = 1.0 / videoFormat.fps;
                        double pos = (Project.SongPosT / (double)Project.SongLengthT);
                        Project.setSongPosS((float)(pos + frameTimeS), false);
                        progress.UpdateProgress((float)Project.NormSongPos);
                    }
                }
                finally
                {
                    Project = backup;
                    endVideoRender();
                    rtCube?.Dispose();
                    for (int i = 0; i < 2; i++) rt32[i]?.Dispose();
                    rt8?.Dispose();
                    if (options.SSAAEnabled) rtFinal?.Dispose();
                }
            }
        }

        void drawVideoFrame(double songPosS, float fps, int frameSamples, VideoExportOptions options,
            RenderTargetCube rtCube, RenderTarget2D[] rt32, RenderTarget2D rt8, Effect cubeToPlaneFx)
        {
            if (!options.Sphere) frameSamples = 1;
            for (int i = 0; i < frameSamples; i++)
            {
                RenderTarget2D rt = null;
                Texture2D prev = i > 0 ? rt32[(i + 1) % 2] : null;
                if (i == frameSamples - 1) rt = rt8;
                drawVideoFrameSample(options, rtCube, rt, prev, cubeToPlaneFx);
                double sampleTime = 1.0 / frameSamples / fps;
                Project.setSongPosS(songPosS + sampleTime, false);
                songPosS += sampleTime;
            }
        }

        void drawVideoFrameSample(VideoExportOptions options, RenderTargetCube rtCube, RenderTarget2D rt,
            Texture2D prevFrame, Effect cubeToPlaneFx)
        {
            graphicsDevice.SetRenderTarget(rt);
            graphicsDevice.Clear(Color.Transparent);
            if (options.Sphere)
            {
                if (options.SphericalStereo)
                {
                    drawSphere(options, rtCube, rt, prevFrame, cubeToPlaneFx, -1);
                    drawSphere(options, rtCube, rt, prevFrame, cubeToPlaneFx, 1);
                }
                else
                    drawSphere(options, rtCube, rt, prevFrame, cubeToPlaneFx);
            }
            else
            {
                Viewport vp = graphicsDevice.Viewport;
                if (options.SphericalStereo)
                {
                    Project.Props.Camera.Eye = -1;
                    graphicsDevice.Viewport = new Viewport(0, 0, vp.Width / 2, vp.Height);
                }
                Project.drawSong();
                if (options.SphericalStereo)
                {
                    Project.Props.Camera.Eye = 1;
                    graphicsDevice.Viewport = new Viewport(vp.Width / 2, 0, vp.Width / 2, vp.Height);
                    Project.drawSong();
                }
            }
        }

        void drawSphere(VideoExportOptions options, RenderTargetCube rtCube, RenderTarget2D rt,
            Texture2D prevFrame, Effect cubeToPlaneFx, int eye = 0)
        {
            Project.Props.Camera.Eye = eye;
            for (int i = 0; i < 6; i++)
            {
                graphicsDevice.SetRenderTarget(rtCube, (CubeMapFace)Enum.ToObject(typeof(CubeMapFace), i));
                Project.Props.Camera.CubeMapFace = i;
                graphicsDevice.Clear(Color.Transparent);
                Project.drawSong();
            }
            Project.Props.Camera.CubeMapFace = -1;
            Project.Props.Camera.Eye = 0;
            cubeToPlaneFx.Parameters["PrevFrame"].SetValue(prevFrame);
            cubeToPlaneFx.Parameters["IsFirstFrame"].SetValue(prevFrame == null);
            graphicsDevice.SetRenderTarget(rt);
            Viewport vp = graphicsDevice.Viewport;

            Vector4 vpBounds;
            Vector2 prevFrameSO;
            if (eye == 0) { vpBounds = new Vector4(0, 0, vp.Width, vp.Height); prevFrameSO = new Vector2(1, 0); }
            else if (eye == 1) { vpBounds = new Vector4(0, 0, vp.Width, vp.Height / 2); prevFrameSO = new Vector2(0.5f, -0.25f); }
            else if (eye == -1) { vpBounds = new Vector4(0, vp.Height / 2, vp.Width, vp.Height / 2); prevFrameSO = new Vector2(0.5f, 0.25f); }
            else throw new Exception("Undefined eye index");

            cubeToPlaneFx.Parameters["ViewportSize"].SetValue(new Vector2(vpBounds.Z, vpBounds.W));
            cubeToPlaneFx.Parameters["PrevFrameScaleOffset"].SetValue(prevFrameSO);
            cubeToPlaneFx.Parameters["FovLimit"].SetValue(options.SphericalStereo ? (float)Math.Cos(150f / 360 * Math.PI) : -1);
            graphicsDevice.Viewport = new Viewport((int)vpBounds.X, (int)vpBounds.Y, (int)vpBounds.Z, (int)vpBounds.W);
            cubeToPlaneFx.CurrentTechnique.Passes[0].Apply();
            quad.draw();
        }

        void endVideoRender()
        {
            graphicsDevice.SetRenderTarget(null);
            Media.endVideoEnc();
            Camera.InvertY = false;
            isRenderingVideo = false;
        }

        public static Color HSLA2RGBA(Vector4 hsla)
        {
            float v;
            float r, g, b;
            float h = hsla.X, s = hsla.Y, l = hsla.Z;
            r = l; g = l; b = l;
            v = (l <= 0.5f) ? (l * (1.0f + s)) : (l + s - l * s);
            if (v > 0)
            {
                float m = l + l - v;
                float sv = (v - m) / v;
                h *= 6.0f;
                int sextant = (int)h;
                float fract = h - sextant;
                float vsf = v * sv * fract;
                float mid1 = m + vsf;
                float mid2 = v - vsf;
                switch (sextant)
                {
                    case 0: r = v; g = mid1; b = m; break;
                    case 1: r = mid2; g = v; b = m; break;
                    case 2: r = m; g = v; b = mid1; break;
                    case 3: r = m; g = mid2; b = v; break;
                    case 4: r = mid1; g = m; b = v; break;
                    case 5: r = v; g = m; b = mid2; break;
                }
            }
            return new Color(r, g, b, hsla.W);
        }
    }
}
