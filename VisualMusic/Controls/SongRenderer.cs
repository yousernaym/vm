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
        GraphicsDevice _graphicsDevice;
        Action<int, int> _resetDevice;
        ServiceContainer _services;
        ContentManager _content;
        SpriteBatch _spriteBatch;
        BlendState _blendState;
        RasterizerState _rastState;
        Effect _postProcessFx;
        ScreenQuad _quad;
        Texture2D _regionSelectTexture;
        Texture2D _backgroundTexture;
        readonly object _renderLock = new object();

        // --- Layout ---
        int _clientWidth = 1;
        int _clientHeight = 1;

        // --- Timing ---
        Stopwatch _stopwatch = new Stopwatch();
        TimeSpan _oldTime = new TimeSpan(0);
        double _deltaTimeS;
        bool _isRenderingVideo = false;
        /// <summary>True while a background video export is using the GraphicsDevice. The live
        /// game loop must skip Draw() during this time to avoid corrupting device state.</summary>
        public bool IsRenderingVideo => _isRenderingVideo;
        const int CmFaceSide = 4096;

        // --- Input state ---
        bool _leftMbPressed;
        bool _rightMbPressed;
        bool _selectingRegion = false;
        bool _mergeRegionSelection = false;
        bool _mousePosScrollSong = false;
        bool _isPausingWhileScrolling = false;
        Rectangle _selectedScreenRegion;
        bool _forceDefaultNoteStyle = false;

        // --- Dependencies ---
        ITrackSelectionService _trackSelection;

        // --- Callbacks ---
        /// <summary>Sets the screen-space cursor position (x,y in host client coordinates).</summary>
        public Action<int, int> SetCursorPosition { get; set; }

        /// <summary>
        /// Called whenever mouse-look mode is toggled.  Argument is <c>true</c> when mode activates,
        /// <c>false</c> when it deactivates.  Wire this on the UI thread (e.g. via Dispatcher).
        /// </summary>
        public Action<bool> OnCameraControlModeChanged { get; set; }

        /// <summary>
        /// Requests a camera reset (Ctrl+R from the focused song panel, which doesn't reach WPF key
        /// bindings). Wire this on the UI thread to invoke the reset-camera command.
        /// </summary>
        public Action OnResetCamera { get; set; }

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
        public bool LeftMbPressed => _leftMbPressed;
        public bool RightMbPressed => _rightMbPressed;
        public bool SelectingRegion => _selectingRegion;
        public SpriteBatch SpriteBatch => _spriteBatch;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        public ContentManager Content => _content;
        public WaveformPanel WaveformPanel { get; private set; } = new WaveformPanel();
        public TimeSpan TotalTimeElapsed => _stopwatch.Elapsed;

        public bool ForceDefaultNoteStyle
        {
            get => _forceDefaultNoteStyle;
            set => _forceDefaultNoteStyle = value;
        }

        public const float SmallScrollStep = 1.0f / 16;
        public const float LargeScrollStep = 1.0f;

        public delegate void Delegate_songPosChanged();
        public Delegate_songPosChanged OnSongPosChanged { get; set; }

        public void SetTrackSelectionService(ITrackSelectionService service) => _trackSelection = service;

        public void Initialize(GraphicsDevice gd, Action<int, int> resetDeviceCallback, ServiceContainer svc, int width, int height)
        {
            _graphicsDevice = gd;
            _resetDevice = resetDeviceCallback;
            _services = svc;
            _clientWidth = Math.Max(1, width);
            _clientHeight = Math.Max(1, height);

            _stopwatch.Start();
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _blendState = BlendState.AlphaBlend;
            _rastState = new RasterizerState()
            {
                MultiSampleAntiAlias = true,
                CullMode = CullMode.None
            };

            _content = new ContentManager(_services, "Content");
            NoteStyle.SetGraphicsDevice(_graphicsDevice);
            NoteStyle.SetContent(_content);
            NoteStyle.SInitAllStyles();
            LyricsFont = _content.Load<SpriteFont>("Font");

            _regionSelectTexture = new Texture2D(_graphicsDevice, 1, 1);
            _regionSelectTexture.SetData(new[] { Color.White });

            _quad = new ScreenQuad(_graphicsDevice);
            _postProcessFx = _content.Load<Effect>("PostProcess");
            _postProcessFx.CurrentTechnique = _postProcessFx.Techniques["Technique1"];

            WaveformPanel.Init(_graphicsDevice, _spriteBatch);
        }

        public void Update(double dt)
        {
            if (Project?.Notes == null || _isRenderingVideo)
                return;

            TimeSpan newTime = _stopwatch.Elapsed;
            _deltaTimeS = (newTime - _oldTime).TotalSeconds;
            _oldTime = newTime;

            SelectRegion();
            // No keyframe-selection UI in WPF yet: keep the playhead keyframe selected so camera
            // movement (WASD/RF) integrates and camera reset targets it.
            Project.SelectKeyFrameAtSongPos();
            Project.Update(_deltaTimeS);
            ScrollSong();
        }

        // ---- Drawing ----

        public string BeginDraw()
        {
            if (_graphicsDevice == null)
                return "No graphics device";

            string err = HandleDeviceReset();
            if (!string.IsNullOrEmpty(err))
                return err;

            Viewport vp = new Viewport
            {
                X = 0,
                Y = 0,
                Width = _graphicsDevice.PresentationParameters.BackBufferWidth,
                Height = _graphicsDevice.PresentationParameters.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            };
            _graphicsDevice.Viewport = vp;
            return null;
        }

        public void Draw()
        {
            if (_graphicsDevice == null) return;
            _graphicsDevice.BlendState = _blendState;
            _graphicsDevice.Clear(Color.Black);
            if (Project == null) return;

            Project.DrawSong();

            if (_selectingRegion && _selectedScreenRegion.Width != 0 && _selectedScreenRegion.Height != 0)
            {
                _spriteBatch.Begin();
                Rectangle normRect = NormalizeRect(_selectedScreenRegion);
                _spriteBatch.Draw(_regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, normRect.Width, 1), Color.White);
                _spriteBatch.Draw(_regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, 1, normRect.Height), Color.White);
                _spriteBatch.Draw(_regionSelectTexture, new Rectangle(normRect.Left, normRect.Bottom, normRect.Width, 1), Color.White);
                _spriteBatch.Draw(_regionSelectTexture, new Rectangle(normRect.Right, normRect.Top, 1, normRect.Height), Color.White);
                _spriteBatch.End();
            }
        }

        public void EndDraw()
        {
            try { _graphicsDevice.Present(); }
            catch { }
        }

        string HandleDeviceReset()
        {
            switch (_graphicsDevice.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Lost:
                    return "Graphics device lost";
                case GraphicsDeviceStatus.NotReset:
                    try { _resetDevice?.Invoke(_clientWidth, _clientHeight); }
                    catch (Exception e) { return "Graphics device reset failed\n\n" + e; }
                    break;
                default:
                    var pp = _graphicsDevice.PresentationParameters;
                    if (_clientWidth != pp.BackBufferWidth || _clientHeight != pp.BackBufferHeight)
                    {
                        try { _resetDevice?.Invoke(_clientWidth, _clientHeight); }
                        catch (Exception e) { return "Graphics device reset failed\n\n" + e; }
                    }
                    break;
            }
            return null;
        }

        public void OnResize(int width, int height)
        {
            _clientWidth = Math.Max(1, width);
            _clientHeight = Math.Max(1, height);
        }

        // ---- Input ----

        public void HandleMouseDown(bool isLeft, bool isShiftHeld, int x, int y)
        {
            if (Project?.Notes == null)
                return;

            // In mouse-look mode the left button is used for roll; right button has no special action.
            if (Camera.MouseRot)
            {
                if (isLeft) _leftMbPressed = true;
                return;
            }

            if (isLeft)
            {
                _leftMbPressed = true;
                if (isShiftHeld)
                    _mergeRegionSelection = true;
                _selectedScreenRegion.X = (int)((NormMouseX * 0.5f + 0.5f) * _clientWidth);
                _selectedScreenRegion.Y = (int)(NormMouseY * _clientHeight);
            }
            else
            {
                _rightMbPressed = true;
                _mousePosScrollSong = true;
                if (Project.IsPlaying)
                {
                    _isPausingWhileScrolling = true;
                    Project.TogglePlayback();
                }
            }
        }

        public void HandleMouseUp(bool isLeft)
        {
            // In mouse-look mode only track left button for roll; suppress normal scroll/select.
            if (Camera.MouseRot)
            {
                if (isLeft) _leftMbPressed = false;
                return;
            }

            if (isLeft)
            {
                _leftMbPressed = false;
                _mergeRegionSelection = false;
            }
            else
            {
                _rightMbPressed = false;
                _mousePosScrollSong = false;
                if (_isPausingWhileScrolling && Project != null && !Project.IsPlaying)
                {
                    Project.TogglePlayback();
                    _isPausingWhileScrolling = false;
                }
            }
        }

        public void HandleMouseMove(int x, int y, int workAreaHeight)
        {
            int middleX = _clientWidth / 2;
            int middleY = _clientHeight / 2;

            NormMouseX = (float)(x - middleX) * 2 / _clientWidth;
            NormMouseY = (float)y / _clientHeight;

            if (Camera.MouseRot)
            {
                NormMouseX = (float)(x - middleX) * 2 / workAreaHeight;
                NormMouseY = (float)(y - middleY) * 2 / workAreaHeight;
                SetCursorPosition?.Invoke(middleX, middleY);
                // Hold left button to roll; otherwise yaw/pitch.
                Project?.GetKeyFrameAtSongPos()?.ProjProps.Camera.ApplyMouseRot(NormMouseX, NormMouseY, _leftMbPressed);
            }
        }

        /// <summary>Returns true if the key was fully handled (suppress further processing).</summary>
        public bool HandleKeyDown(int vkCode)
        {
            var keyFrame = Project?.GetKeyFrameAtSongPos();
            if (keyFrame == null) return false;

            bool suppress = false;
            var key = (WinKeys)vkCode;
            var modifiers = System.Windows.Forms.Control.ModifierKeys;

            // Escape exits mouse-look mode.
            if (key == WinKeys.Escape && Camera.MouseRot)
            {
                SetMouseLook(false);
                return true;
            }

            if (keyFrame.ProjProps.Camera.Control(key, true, modifiers))
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
                Project?.TogglePlayback();
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
                            Project.TrackViews[t].CreateGeo(Project, Project.GlobalTrackProps);
                            tprops.StyleProps.Type = currentNoteStyle;
                        }
                    }
                }
            }

            return suppress;
        }

        /// <summary>Resets camera velocity when a movement/rotation key is released.</summary>
        public void HandleKeyUp(int vkCode)
        {
            var keyFrame = Project?.GetKeyFrameAtSongPos();
            if (keyFrame == null) return;

            var key = (WinKeys)vkCode;
            var modifiers = System.Windows.Forms.Control.ModifierKeys;

            if (keyFrame.ProjProps.Camera.Control(key, false, modifiers))
            {
                foreach (var other in Project.KeyFrames.Values)
                {
                    if (keyFrame != other && other.Selected)
                    {
                        other.ProjProps.Camera.Pos = keyFrame.ProjProps.Camera.Pos;
                        other.ProjProps.Camera.Orientation = keyFrame.ProjProps.Camera.Orientation;
                    }
                }
            }
        }

        /// <summary>Toggles mouse-look mode on/off (e.g. triggered by middle-mouse click).</summary>
        public void ToggleMouseLook() => SetMouseLook(!Camera.MouseRot);

        void SetMouseLook(bool on)
        {
            if (Camera.MouseRot == on) return;
            Camera.MouseRot = on;
            OnCameraControlModeChanged?.Invoke(on);
        }

        // ---- Selection ----

        void SelectRegion()
        {
            if (_trackSelection == null || _trackSelection.TrackListCount == 0)
                return;

            // In mouse-look mode the left button rolls the camera, so it must not start a region selection.
            if (_leftMbPressed && !Camera.MouseRot)
            {
                _selectingRegion = true;

                Point mousePos = new Point(
                    (int)((NormMouseX * 0.5f + 0.5f) * _clientWidth),
                    (int)(NormMouseY * _clientHeight));
                _selectedScreenRegion.Width = mousePos.X - _selectedScreenRegion.X;
                _selectedScreenRegion.Height = mousePos.Y - _selectedScreenRegion.Y;

                if (_selectedScreenRegion.Width == 0 || _selectedScreenRegion.Height == 0)
                    return;

                RectangleF normScreenSelection = new RectangleF(
                    (float)_selectedScreenRegion.X / _clientWidth,
                    (float)_selectedScreenRegion.Y / _clientHeight,
                    (float)_selectedScreenRegion.Width / _clientWidth,
                    (float)_selectedScreenRegion.Height / _clientHeight);
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
                        Project.TrackViews[i].Geo.AreObjectsInFrustum(selectionFrustum, Project.SongPosP - Project.PlaybackOffsetP, Project, Project.TrackViews[i].TrackProps))
                    {
                        _trackSelection.SetTrackSelected(i, true);
                        selectedCount++;
                    }
                    else if (!_mergeRegionSelection && _trackSelection.TrackListCount > 1)
                        _trackSelection.SetTrackSelected(i, false);
                }

                if (selectedCount == 0 && !_mergeRegionSelection)
                    _trackSelection.SetTrackSelected(0, true);
                else if (selectedCount > 0)
                    _trackSelection.SetTrackSelected(0, false);
            }
            else if (_selectingRegion)
            {
                _selectingRegion = false;
            }
        }

        Rectangle NormalizeRect(Rectangle r)
        {
            if (r.Height < 0) { r.Y += r.Height; r.Height = -r.Height; }
            if (r.Width < 0) { r.X += r.Width; r.Width = -r.Width; }
            return r;
        }

        // ---- Scroll ----

        public void ScrollSong()
        {
            if (_mousePosScrollSong && !_selectingRegion && Project != null)
                Project.NormSongPos += (float)(Math.Pow(NormMouseX, 2) * Math.Sign(NormMouseX) * _deltaTimeS * 0.3f);
        }

        // ---- Background image ----

        public void LoadBackgroundImage(string path)
        {
            UnloadBackgroundImage();
            if (!string.IsNullOrWhiteSpace(path))
            {
                try { _backgroundTexture = Texture2D.FromFile(_graphicsDevice, path); }
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
            _backgroundTexture?.Dispose();
            _backgroundTexture = null;
        }

        public void DrawBackground()
        {
            _postProcessFx.Parameters["saturationLevel"].SetValue(Project.Props.BackgroundImageSaturation);
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _postProcessFx, null);
            if (_backgroundTexture != null)
                _spriteBatch.Draw(_backgroundTexture,
                    new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height),
                    new Color(Project.Props.BackgroundImageOpacity, Project.Props.BackgroundImageOpacity, Project.Props.BackgroundImageOpacity));
            _spriteBatch.End();
        }

        public void InitFrame()
        {
            _graphicsDevice.RasterizerState = _rastState;
        }

        // ---- ISongDrawHost extras ----
        public int ClientWidth  => _clientWidth;
        public int ClientHeight => _clientHeight;
        // TotalTimeElapsed already declared above (line ~86).
        /// <summary>No-op: WPF render loop is continuous — no explicit invalidation needed.</summary>
        public void Invalidate() { }
        public void NotifySongPosChanged() => OnSongPosChanged?.Invoke();

        public void UpdateTimeStamp()
        {
            _oldTime = _stopwatch.Elapsed;
        }

        // ---- Video rendering ----

        public void RenderVideo(string videoFilePath, IRenderProgressCallback progress, VideoExportOptions options)
        {
            lock (_renderLock)
            {
                VideoFormat videoFormat = new VideoFormat((uint)options.Width, (uint)options.Height);
                videoFormat.fps = options.Fps;
                if (!Media.BeginVideoEnc(videoFilePath, Project.AudioFilePath, videoFormat,
                    Project.Props.AudioOffset + Project.Props.PlaybackOffsetS,
                    options.Sphere && options.SphericalMetadata, options.SphericalStereo,
                    options.VideoCodec, options.VideoCrf))
                {
                    lock (progress.CancelLock) { }
                    progress.ShowMessage("Couldn't initialize video encoding.");
                    return;
                }

                _isRenderingVideo = true;
                RenderTarget2D[] rt32 = new RenderTarget2D[2];
                RenderTargetCube rtCube = null;
                RenderTarget2D rt8 = null;
                RenderTarget2D rtFinal = null;
                Effect cubeToPlaneFx = null;
                Effect ssFx = null;
                uint[] frameData = null;
                Project backup = Project;
                Project = Project.Clone();

                try
                {
                    try
                    {
                        if (options.Sphere)
                        {
                            rtCube = new RenderTargetCube(_graphicsDevice, CmFaceSide, true,
                                SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
                            cubeToPlaneFx = _content.Load<Effect>("CubeToPlane");
                            cubeToPlaneFx.Parameters["CubeMap"].SetValue(rtCube);
                            cubeToPlaneFx.Parameters["FrameSamples"].SetValue((float)1);
                        }
                        rt8 = new RenderTarget2D(_graphicsDevice, options.SSAAWidth, options.SSAAHeight,
                            options.SSAAEnabled, SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
                        rtFinal = options.SSAAEnabled
                            ? new RenderTarget2D(_graphicsDevice, options.Width, options.Height, false,
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

                    ssFx = _content.Load<Effect>("ss");
                    Camera.InvertY = options.Sphere;

                    int frameSamples = 1;
                    double songPosS = 0;
                    Project.SetSongPosS(0, false);

                    while (Project.NormSongPos < 1 && !progress.Cancel)
                    {
                        Project.InterpolateFrames();
                        DrawVideoFrame(songPosS, videoFormat.fps, frameSamples, options, rtCube, rt32, rt8, cubeToPlaneFx);
                        if (options.SSAAEnabled)
                        {
                            _graphicsDevice.SetRenderTarget(rtFinal);
                            _graphicsDevice.Clear(Color.Transparent);
                            ssFx.Parameters["FrameTex"].SetValue(rt8);
                            ssFx.CurrentTechnique.Passes[0].Apply();
                            _quad.Draw();
                        }
                        _graphicsDevice.SetRenderTarget(null);
                        rtFinal.GetData<uint>(frameData);
                        if (!Media.WriteFrame(frameData))
                        {
                            progress.ShowMessage("Couldn't add frame");
                            break;
                        }
                        // Advance the playhead by one frame's worth of *seconds* (setSongPosS takes
                        // seconds). songPosS is the running second-accumulator; the earlier code used a
                        // normalized [0,1] position here, which pinned every frame to the song start.
                        double frameTimeS = 1.0 / videoFormat.fps;
                        Project.SetSongPosS(songPosS + frameTimeS, false);
                        songPosS += frameTimeS;
                        progress.UpdateProgress((float)Project.NormSongPos);
                    }
                }
                finally
                {
                    Project = backup;
                    EndVideoRender();
                    rtCube?.Dispose();
                    for (int i = 0; i < 2; i++) rt32[i]?.Dispose();
                    rt8?.Dispose();
                    if (options.SSAAEnabled) rtFinal?.Dispose();
                }
            }
        }

        void DrawVideoFrame(double songPosS, float fps, int frameSamples, VideoExportOptions options,
            RenderTargetCube rtCube, RenderTarget2D[] rt32, RenderTarget2D rt8, Effect cubeToPlaneFx)
        {
            if (!options.Sphere) frameSamples = 1;
            for (int i = 0; i < frameSamples; i++)
            {
                RenderTarget2D rt = null;
                Texture2D prev = i > 0 ? rt32[(i + 1) % 2] : null;
                if (i == frameSamples - 1) rt = rt8;
                DrawVideoFrameSample(options, rtCube, rt, prev, cubeToPlaneFx);
                double sampleTime = 1.0 / frameSamples / fps;
                Project.SetSongPosS(songPosS + sampleTime, false);
                songPosS += sampleTime;
            }
        }

        void DrawVideoFrameSample(VideoExportOptions options, RenderTargetCube rtCube, RenderTarget2D rt,
            Texture2D prevFrame, Effect cubeToPlaneFx)
        {
            _graphicsDevice.SetRenderTarget(rt);
            _graphicsDevice.Clear(Color.Transparent);
            if (options.Sphere)
            {
                if (options.SphericalStereo)
                {
                    DrawSphere(options, rtCube, rt, prevFrame, cubeToPlaneFx, -1);
                    DrawSphere(options, rtCube, rt, prevFrame, cubeToPlaneFx, 1);
                }
                else
                    DrawSphere(options, rtCube, rt, prevFrame, cubeToPlaneFx);
            }
            else
            {
                Viewport vp = _graphicsDevice.Viewport;
                if (options.SphericalStereo)
                {
                    Project.Props.Camera.Eye = -1;
                    _graphicsDevice.Viewport = new Viewport(0, 0, vp.Width / 2, vp.Height);
                }
                Project.DrawSong();
                if (options.SphericalStereo)
                {
                    Project.Props.Camera.Eye = 1;
                    _graphicsDevice.Viewport = new Viewport(vp.Width / 2, 0, vp.Width / 2, vp.Height);
                    Project.DrawSong();
                }
            }
        }

        void DrawSphere(VideoExportOptions options, RenderTargetCube rtCube, RenderTarget2D rt,
            Texture2D prevFrame, Effect cubeToPlaneFx, int eye = 0)
        {
            Project.Props.Camera.Eye = eye;
            for (int i = 0; i < 6; i++)
            {
                _graphicsDevice.SetRenderTarget(rtCube, (CubeMapFace)Enum.ToObject(typeof(CubeMapFace), i));
                Project.Props.Camera.CubeMapFace = i;
                _graphicsDevice.Clear(Color.Transparent);
                Project.DrawSong();
            }
            Project.Props.Camera.CubeMapFace = -1;
            Project.Props.Camera.Eye = 0;
            cubeToPlaneFx.Parameters["PrevFrame"].SetValue(prevFrame);
            cubeToPlaneFx.Parameters["IsFirstFrame"].SetValue(prevFrame == null);
            _graphicsDevice.SetRenderTarget(rt);
            Viewport vp = _graphicsDevice.Viewport;

            Vector4 vpBounds;
            Vector2 prevFrameSO;
            if (eye == 0) { vpBounds = new Vector4(0, 0, vp.Width, vp.Height); prevFrameSO = new Vector2(1, 0); }
            else if (eye == 1) { vpBounds = new Vector4(0, 0, vp.Width, vp.Height / 2); prevFrameSO = new Vector2(0.5f, -0.25f); }
            else if (eye == -1) { vpBounds = new Vector4(0, vp.Height / 2, vp.Width, vp.Height / 2); prevFrameSO = new Vector2(0.5f, 0.25f); }
            else throw new Exception("Undefined eye index");

            cubeToPlaneFx.Parameters["ViewportSize"].SetValue(new Vector2(vpBounds.Z, vpBounds.W));
            cubeToPlaneFx.Parameters["PrevFrameScaleOffset"].SetValue(prevFrameSO);
            cubeToPlaneFx.Parameters["FovLimit"].SetValue(options.SphericalStereo ? (float)Math.Cos(150f / 360 * Math.PI) : -1);
            _graphicsDevice.Viewport = new Viewport((int)vpBounds.X, (int)vpBounds.Y, (int)vpBounds.Z, (int)vpBounds.W);
            cubeToPlaneFx.CurrentTechnique.Passes[0].Apply();
            _quad.Draw();
        }

        void EndVideoRender()
        {
            _graphicsDevice.SetRenderTarget(null);
            Media.EndVideoEnc();
            Camera.InvertY = false;
            _isRenderingVideo = false;
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
