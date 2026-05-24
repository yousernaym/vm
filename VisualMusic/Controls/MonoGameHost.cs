using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using WinFormsGraphicsDevice;

namespace VisualMusic
{
    /// <summary>
    /// WPF HwndHost that creates a Win32 child window, binds MonoGame's GraphicsDevice to it,
    /// and drives SongRenderer at 120 fps via a DispatcherTimer.
    /// Input (mouse, keyboard) is received via the overridden WndProc.
    /// </summary>
    public class MonoGameHost : HwndHost
    {
        // ---- Win32 P/Invoke ----
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateWindowEx(uint exStyle, string className, string windowName,
            uint style, int x, int y, int width, int height,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        const uint WS_CHILD = 0x40000000;
        const uint WS_VISIBLE = 0x10000000;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOACTIVATE = 0x0010;

        // ---- Win32 messages ----
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_KEYDOWN = 0x0100;
        const int WM_SIZE = 0x0005;

        const int MK_SHIFT = 0x0004;

        // ---- Fields ----
        IntPtr _hwnd;
        GraphicsDeviceService _gds;
        DispatcherTimer _timer;
        Stopwatch _clock = new Stopwatch();
        TimeSpan _oldTime;

        // ---- Public ----
        public SongRenderer Renderer { get; private set; }

        public SongRenderer.Delegate_songPosChanged OnSongPosChanged
        {
            get => Renderer?.OnSongPosChanged;
            set { if (Renderer != null) Renderer.OnSongPosChanged = value; }
        }

        // ---- HwndHost ----

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            int w = Math.Max(1, (int)ActualWidth);
            int h = Math.Max(1, (int)ActualHeight);

            _hwnd = CreateWindowEx(0, "STATIC", "",
                WS_CHILD | WS_VISIBLE,
                0, 0, w, h,
                hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
                throw new InvalidOperationException($"CreateWindowEx failed: {Marshal.GetLastWin32Error()}");

            var svc = new ServiceContainer();
            _gds = GraphicsDeviceService.AddRef(_hwnd, w, h);
            svc.AddService<IGraphicsDeviceService>(_gds);

            Renderer = new SongRenderer();
            Renderer.SetCursorPosition = SetCursorFromHostCoords;
            Renderer.Initialize(_gds.GraphicsDevice, _gds.ResetDevice, svc, w, h);

            _clock.Start();
            _timer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / 120)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            return new HandleRef(this, _hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _timer?.Stop();
            _timer = null;
            _gds?.Release(true);
            _gds = null;
            if (hwnd.Handle != IntPtr.Zero)
                DestroyWindow(hwnd.Handle);
        }

        // ---- Game loop ----

        void OnTimerTick(object sender, EventArgs e)
        {
            if (Renderer == null) return;

            TimeSpan now = _clock.Elapsed;
            double dt = (now - _oldTime).TotalSeconds;
            _oldTime = now;

            Renderer.Update(dt);

            string err = Renderer.BeginDraw();
            if (err == null)
            {
                Renderer.Draw();
                Renderer.EndDraw();
            }
        }

        // ---- Resize ----

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);
            if (_hwnd == IntPtr.Zero || _gds == null) return;

            int w = Math.Max(1, (int)info.NewSize.Width);
            int h = Math.Max(1, (int)info.NewSize.Height);

            SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, w, h, SWP_NOZORDER | SWP_NOACTIVATE);
            _gds.ResetDevice(w, h);
            Renderer?.OnResize(w, h);
        }

        // ---- Input via WndProc ----

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (Renderer == null)
                return base.WndProc(hwnd, msg, wParam, lParam, ref handled);

            long lp = lParam.ToInt64();
            int lo = unchecked((short)(lp & 0xFFFF));
            int hi = unchecked((short)((lp >> 16) & 0xFFFF));

            switch (msg)
            {
                case WM_LBUTTONDOWN:
                    bool isShift = (wParam.ToInt32() & MK_SHIFT) != 0;
                    Renderer.HandleMouseDown(true, isShift, lo, hi);
                    break;

                case WM_RBUTTONDOWN:
                    Renderer.HandleMouseDown(false, false, lo, hi);
                    break;

                case WM_LBUTTONUP:
                    Renderer.HandleMouseUp(true);
                    break;

                case WM_RBUTTONUP:
                    Renderer.HandleMouseUp(false);
                    break;

                case WM_MOUSEMOVE:
                    int workH = (int)SystemParameters.WorkArea.Height;
                    Renderer.HandleMouseMove(lo, hi, workH);
                    break;

                case WM_KEYDOWN:
                    int vk = wParam.ToInt32();
                    bool suppress = Renderer.HandleKeyDown(vk);
                    if (suppress) handled = true;
                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        // ---- Cursor helper ----

        void SetCursorFromHostCoords(int clientX, int clientY)
        {
            try
            {
                System.Windows.Point screenPt = PointToScreen(new System.Windows.Point(clientX, clientY));
                System.Windows.Forms.Cursor.Position =
                    new System.Drawing.Point((int)screenPt.X, (int)screenPt.Y);
            }
            catch { }
        }
    }
}
