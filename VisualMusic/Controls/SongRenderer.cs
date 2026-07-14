using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using VisualMusic.Keyframes;
using VisualMusic.MonoGameInterop;

namespace VisualMusic
{
    using RectangleF = System.Drawing.RectangleF;

    /// <summary>
    /// MonoGame renderer hosted by MonoGameHost.
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
        // Background texture(s) — single texture (LoadBackgroundImage) or crossfade pair (SetBackgroundCrossfade)
        readonly System.Collections.Generic.Dictionary<string, Texture2D> _bkgCache
            = new System.Collections.Generic.Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        Texture2D _bkgTexA;   // "from" texture (or the single texture when no crossfade)
        Texture2D _bkgTexB;   // "to" texture (null when not crossfading)
        float _bkgBlend;  // 0 = fully A, 1 = fully B
        bool _bkgFadeToEmpty; // true when crossfading from an image to no background
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

        /// <summary>
        /// Ctrl+mouse-wheel over the focused song panel: adjusts the viewport width of the selected
        /// track(s). Argument is the number of wheel notches (positive = wheel away / zoom in). Wire on
        /// the UI thread.
        /// </summary>
        public Action<int> OnCtrlWheelViewWidth { get; set; }

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

        /// <summary>
        /// Handles a mouse-wheel notch. Ctrl+wheel adjusts the selected track(s) viewport width.
        /// Returns true if the message was consumed.
        /// </summary>
        public bool HandleMouseWheel(int wheelDelta, bool isCtrl)
        {
            if (!isCtrl || Project?.Notes == null || OnCtrlWheelViewWidth == null)
                return false;
            int notches = wheelDelta / 120;
            if (notches == 0)
                notches = Math.Sign(wheelDelta);
            if (notches != 0)
                OnCtrlWheelViewWidth.Invoke(notches);
            return true;
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
                Project?.Props.Camera.ApplyMouseRot(NormMouseX, NormMouseY, _leftMbPressed);
            }
        }

        // ---- Camera keyframe edit interception ----
        // The camera is keyframeable in the new per-property model (id "Camera", project scope), but it is
        // edited through movement keys / mouse-look rather than a WPF control, so the blue-control prompt is
        // applied here at the input source.
        const string CameraPropId = "Camera";
        bool _cameraEditDenied;   // user answered "No" for the current key-hold; suppress until key-up
        bool _cameraPromptOpen;   // re-entrancy guard so the modal prompt can't stack

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        /// <summary>True if the key is currently physically held (used after a modal that ate the key-up).</summary>
        static bool IsKeyPhysicallyDown(int vkCode) => (GetAsyncKeyState(vkCode) & 0x8000) != 0;

        static bool IsCameraKey(Key key)
            => key is Key.W or Key.A or Key.S or Key.D
                   or Key.R or Key.F or Key.Q or Key.E;

        /// <summary>True if the key+modifiers form a camera move/rotate gesture (mirrors Camera.Control gating).</summary>
        static bool IsCameraGesture(Key key, ModifierKeys modifiers)
        {
            if (modifiers != ModifierKeys.None && modifiers != ModifierKeys.Shift) return false;
            return IsCameraKey(key);
        }

        /// <summary>
        /// Gate for a camera edit (movement key or mouse-look). Returns true if the edit may proceed.
        /// When the camera is blue (keyframes elsewhere but not here) it pauses playback and prompts;
        /// <paramref name="prompted"/> reports whether the modal dialog was shown.
        /// </summary>
        bool AllowCameraEdit(out bool prompted)
        {
            prompted = false;
            if (_cameraEditDenied) return false;

            var scope = KeyframeService.KfScope.Project;
            if (KeyframeService.HasKeyHereForAll(CameraPropId, scope)) return true; // green
            if (!KeyframeService.HasAnyKeyForAny(CameraPropId, scope)) return true; // no camera keyframes
            if (_cameraPromptOpen) return false;                                    // re-entrant

            // blue → pause and prompt
            prompted = true;
            KeyframeService.PausePlayback();
            _cameraPromptOpen = true;
            try
            {
                bool ok = KeyframeService.EnsureKeyframeForEdit(CameraPropId, scope);
                if (!ok) _cameraEditDenied = true;  // remember "No" for the rest of this key-hold
                return ok;
            }
            finally { _cameraPromptOpen = false; }
        }

        /// <summary>Returns true if the key was fully handled (suppress further processing).</summary>
        public bool HandleKeyDown(int vkCode)
        {
            if (Project?.Notes == null) return false;

            bool suppress = false;
            var key = KeyInterop.KeyFromVirtualKey(vkCode);
            var modifiers = Keyboard.Modifiers;
            var camera = Project.Props.Camera;

            // Escape exits mouse-look mode.
            if (key == Key.Escape && Camera.MouseRot)
            {
                SetMouseLook(false);
                return true;
            }

            // Intercept camera movement/rotation when the camera is keyframed but not at this position.
            if (IsCameraGesture(key, modifiers))
            {
                if (!AllowCameraEdit(out bool prompted))
                    return true;   // suppressed (declined, or re-entrant during the prompt)

                // The modal prompt steals the key-up, so a key that was tapped (released while the dialog
                // was open) would otherwise start velocity that never stops. Only begin movement if the
                // key is still physically held after the prompt closes.
                if (prompted && !IsKeyPhysicallyDown(vkCode))
                    return true;
            }

            if (camera.Control(key, true, modifiers))
            {
                suppress = true;
                Project.SyncLiveCameraEdit();
            }

            if (key == Key.Space)
            {
                Project?.TogglePlayback();
                suppress = true;
            }

            if (key == Key.Z)
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
            if (Project?.Notes == null) return;

            var key = KeyInterop.KeyFromVirtualKey(vkCode);
            var modifiers = Keyboard.Modifiers;
            var camera = Project.Props.Camera;

            // Releasing a camera key clears a prior "No" so the next press can prompt again.
            if (IsCameraKey(key))
                _cameraEditDenied = false;

            if (camera.Control(key, false, modifiers))
            {
                Project.SyncLiveCameraEdit();
            }
        }

        /// <summary>Toggles mouse-look mode on/off (e.g. triggered by middle-mouse click).</summary>
        public void ToggleMouseLook() => SetMouseLook(!Camera.MouseRot);

        void SetMouseLook(bool on)
        {
            if (Camera.MouseRot == on) return;
            // Entering mouse-look is a camera edit — gate it the same way as movement keys.
            if (on && !AllowCameraEdit(out _)) return;
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
                        Project.TrackViews[i].Geo.AreObjectsInFrustum(selectionFrustum, Project.GetSongPosP(Project.TrackViews[i].TrackProps), Project, Project.TrackViews[i].TrackProps))
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

        /// <summary>Loads a texture from <paramref name="path"/> into the cache, or returns a cached copy.</summary>
        Texture2D GetOrLoadCachedTexture(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (_bkgCache.TryGetValue(path, out var cached)) return cached;
            try
            {
                var tex = Texture2D.FromFile(_graphicsDevice, path);
                _bkgCache[path] = tex;
                return tex;
            }
            catch (Exception ex)
            {
                MetroMessageBox.Show(
                    $"Could not load image \"{Path.GetFileName(path)}\". {ex.Message}",
                    "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return null;
            }
        }

        public void LoadBackgroundImage(string path)
        {
            // Clear the crossfade pair; set the single base texture.
            _bkgTexB = null;
            _bkgBlend = 0f;
            _bkgFadeToEmpty = false;
            _bkgTexA = GetOrLoadCachedTexture(path);
            // Purge cache entries that are no longer needed (keep only the active texture)
            PurgeCacheExcept(path);
        }

        public void UnloadBackgroundImage()
        {
            _bkgTexA = null;
            _bkgTexB = null;
            _bkgBlend = 0f;
            _bkgFadeToEmpty = false;
            foreach (var tex in _bkgCache.Values) tex?.Dispose();
            _bkgCache.Clear();
        }

        /// <summary>
        /// Sets up a crossfade between two images for keyframe-driven transitions.
        /// Called every frame from <see cref="Project.InterpolatePropertyKeyframes"/>.
        /// When <paramref name="pathB"/> is empty (but not null) the blend fades A out to transparent;
        /// null pathB means no crossfade (hold / single image).
        /// </summary>
        public void SetBackgroundCrossfade(string pathA, string pathB, float blend)
        {
            _bkgTexA = GetOrLoadCachedTexture(pathA);
            _bkgTexB = string.IsNullOrWhiteSpace(pathB) ? null : GetOrLoadCachedTexture(pathB);
            _bkgBlend = blend;
            _bkgFadeToEmpty = pathB != null && string.IsNullOrWhiteSpace(pathB);
        }

        void PurgeCacheExcept(string keepPath)
        {
            var toRemove = new System.Collections.Generic.List<string>();
            foreach (var kv in _bkgCache)
            {
                if (!string.Equals(kv.Key, keepPath, StringComparison.OrdinalIgnoreCase))
                    toRemove.Add(kv.Key);
            }
            foreach (var key in toRemove)
            {
                _bkgCache[key]?.Dispose();
                _bkgCache.Remove(key);
            }
        }

        public void DrawBackground()
        {
            _postProcessFx.Parameters["saturationLevel"].SetValue(Project.Props.BackgroundImageSaturation);
            var viewport = new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            float opacity = Project.Props.BackgroundImageOpacity;

            // Draw texA (the "from" image, or the single static texture)
            if (_bkgTexA != null)
            {
                float alphaA = opacity;
                if (_bkgTexB != null || _bkgFadeToEmpty)
                    alphaA = opacity * (1f - _bkgBlend);
                int ca = (int)(alphaA * 255f + 0.5f);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _postProcessFx, null);
                _spriteBatch.Draw(_bkgTexA, viewport, new Color(ca, ca, ca, ca));
                _spriteBatch.End();
            }

            // Draw texB (the "to" image) blended on top
            if (_bkgTexB != null && _bkgBlend > 0f)
            {
                float alphaB = opacity * _bkgBlend;
                int cb = (int)(alphaB * 255f + 0.5f);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _postProcessFx, null);
                _spriteBatch.Draw(_bkgTexB, viewport, new Color(cb, cb, cb, cb));
                _spriteBatch.End();
            }
        }

        public void InitFrame()
        {
            _graphicsDevice.RasterizerState = _rastState;
        }

        // ---- ISongDrawHost extras ----
        public int ClientWidth => _clientWidth;
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
                // Export frames must be frame-exact: render waveforms inline on this thread instead
                // of the live loop's async worker (which draws the latest *finished* frame).
                WaveformPanel.Synchronous = true;
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
                        MetroMessageBox.Show(e.Message);
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
                        Project.InterpolatePropertyKeyframes();
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
                    WaveformPanel.Synchronous = false;
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
