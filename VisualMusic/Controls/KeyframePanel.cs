using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VisualMusic.Keyframes;

namespace VisualMusic.Controls
{
    /// <summary>
    /// A thin black panel drawn above the MonoGame host that shows per-property keyframes
    /// as diamond markers.  The playhead is always at horizontal centre; the view scrolls
    /// automatically as the song position changes.
    /// <para>
    /// Left-click a diamond → seeks playback to that tick.
    /// Shift + left-drag a diamond → moves the whole tick column to a new position.
    /// </para>
    /// </summary>
    public sealed class KeyframePanel : FrameworkElement
    {
        // ---- Appearance constants ----
        const double DiamondRadius   = 5.0;
        const double HitRadius       = 8.0;
        const double PlayheadHalfW   = 1.0;

        static readonly Brush BackBrush         = Brushes.Black;
        static readonly Brush DiamondFill       = Brushes.White;
        static readonly Brush DiamondFillAt     = Brushes.Yellow;   // keyframe at current tick
        static readonly Pen   DiamondPen        = Freeze(new Pen(Brushes.Gray,   1.0));
        static readonly Pen   DiamondPenAt      = Freeze(new Pen(Brushes.White,  1.0));
        static readonly Pen   PlayheadPen       = Freeze(new Pen(Brushes.White,  1.5));

        static T Freeze<T>(T obj) where T : Freezable { obj.Freeze(); return obj; }

        // ---- Drag state ----

        bool _dragging;
        int  _dragSourceTick;
        double _dragCurrentX;          // last mouse X during drag
        List<(int tick, double cx)> _hitTargets = new();  // populated during render

        // ---- Constructor ----

        Window _window;

        public KeyframePanel()
        {
            ClipToBounds = true;
            // Background is painted directly in OnRender (FrameworkElement has no Background property)

            KeyframeService.RefreshRequested  += OnRefresh;
            KeyframeService.KeyframesChanged  += OnRefresh;
            Loaded   += OnLoaded;
            Unloaded += OnUnloaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            _window = Window.GetWindow(this);
            if (_window == null) return;
            _window.PreviewMouseMove += OnWindowPreviewMouseMove;
            _window.PreviewKeyDown   += OnWindowPreviewKeyDown;
            _window.PreviewKeyUp     += OnWindowPreviewKeyUp;
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_window != null)
            {
                _window.PreviewMouseMove -= OnWindowPreviewMouseMove;
                _window.PreviewKeyDown   -= OnWindowPreviewKeyDown;
                _window.PreviewKeyUp     -= OnWindowPreviewKeyUp;
                _window = null;
            }
            if (!_dragging)
                Mouse.OverrideCursor = null;
        }

        void OnRefresh() => InvalidateVisual();

        // ---- Rendering ----

        protected override void OnRender(DrawingContext dc)
        {
            double w   = ActualWidth;
            double h   = ActualHeight;
            double cy  = h / 2.0;

            dc.DrawRectangle(BackBrush, null, new Rect(0, 0, w, h));

            var proj = KeyframeService.Project;
            if (proj == null || proj.SongLengthT <= 0) return;

            double songPosT  = proj.SongPosT;
            double viewWidthT = proj.ViewWidthT;
            if (viewWidthT <= 0) return;

            double scale = w / viewWidthT;  // pixels per tick

            _hitTargets.Clear();

            // Draw all keyframe ticks
            foreach (int tick in proj.PropertyKeyframes.AllTicks())
            {
                double cx = w / 2.0 + (tick - songPosT) * scale;

                // Cull off-screen (with margin for the diamond radius)
                if (cx < -DiamondRadius * 2 || cx > w + DiamondRadius * 2) continue;

                bool atCurrentTick = (tick == (int)songPosT);
                var fill = atCurrentTick ? DiamondFillAt : DiamondFill;
                var pen  = atCurrentTick ? DiamondPenAt  : DiamondPen;

                // Diamond shape (rotated square)
                double r = DiamondRadius;
                var points = new PathFigure(new Point(cx, cy - r), new PathSegment[]
                {
                    new LineSegment(new Point(cx + r, cy    ), true),
                    new LineSegment(new Point(cx,     cy + r), true),
                    new LineSegment(new Point(cx - r, cy    ), true),
                }, closed: true);

                dc.DrawGeometry(fill, pen, new PathGeometry(new[] { points }));

                _hitTargets.Add((tick, cx));
            }

            // Draw drag preview
            if (_dragging && viewWidthT > 0)
            {
                double dragCx = _dragCurrentX;
                double r = DiamondRadius + 1;
                var dragPen  = new Pen(Brushes.Orange, 1.5);
                var points = new PathFigure(new Point(dragCx, cy - r), new PathSegment[]
                {
                    new LineSegment(new Point(dragCx + r, cy    ), true),
                    new LineSegment(new Point(dragCx,     cy + r), true),
                    new LineSegment(new Point(dragCx - r, cy    ), true),
                }, closed: true);
                dc.DrawGeometry(null, dragPen, new PathGeometry(new[] { points }));
            }

            // Draw playhead centre line
            dc.DrawLine(PlayheadPen, new Point(w / 2.0, 0), new Point(w / 2.0, h));
        }

        // ---- Hit-testing ----

        int? HitTest(double mouseX)
        {
            double best = HitRadius;
            int? bestTick = null;
            foreach (var (tick, cx) in _hitTargets)
            {
                double dist = Math.Abs(mouseX - cx);
                if (dist < best) { best = dist; bestTick = tick; }
            }
            return bestTick;
        }

        double ScreenXToTick(double mouseX)
        {
            var proj = KeyframeService.Project;
            if (proj == null || proj.ViewWidthT <= 0) return 0;
            return proj.SongPosT + (mouseX - ActualWidth / 2.0) / ActualWidth * proj.ViewWidthT;
        }

        // ---- Mouse events ----

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var pos   = e.GetPosition(this);
            bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            int? hit  = HitTest(pos.X);

            if (hit.HasValue)
            {
                if (shift)
                {
                    // Begin shift-drag
                    _dragging       = true;
                    _dragSourceTick = hit.Value;
                    _dragCurrentX   = pos.X;
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    CaptureMouse();
                }
                else
                {
                    // Click → seek
                    SeekToTick(hit.Value);
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                _dragCurrentX = e.GetPosition(this).X;
                InvalidateVisual();
                e.Handled = true;
                return;
            }
            UpdateCursorFromMousePosition(e.GetPosition(this));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_dragging)
                Mouse.OverrideCursor = null;
        }

        // HwndHost (MonoGame) does not raise WPF MouseLeave when the pointer moves straight down
        // off the strip, so track cursor state at window level too.
        void OnWindowPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsVisible)
            {
                if (!_dragging) Mouse.OverrideCursor = null;
                return;
            }
            UpdateCursorFromMousePosition(e.GetPosition(this));
        }

        void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsVisible) return;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                UpdateCursorFromMousePosition(Mouse.GetPosition(this));
        }

        void OnWindowPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!IsVisible) return;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                UpdateCursorFromMousePosition(Mouse.GetPosition(this));
        }

        void UpdateCursorFromMousePosition(Point pos)
        {
            if (_dragging)
            {
                Mouse.OverrideCursor = Cursors.SizeWE;
                return;
            }
            if (IsVisible && pos.X >= 0 && pos.X <= ActualWidth && pos.Y >= 0 && pos.Y <= ActualHeight)
                UpdateCursor(pos.X);
            else
                Mouse.OverrideCursor = null;
        }

        // Over a marker: SizeWE while Shift is held (drag), otherwise a hand (click-to-seek).
        void UpdateCursor(double mouseX)
        {
            if (HitTest(mouseX).HasValue)
                Mouse.OverrideCursor = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? Cursors.SizeWE : Cursors.Hand;
            else
                Mouse.OverrideCursor = null;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!_dragging) return;

            ReleaseMouseCapture();
            _dragging = false;
            UpdateCursorFromMousePosition(Mouse.GetPosition(this));

            double newTickD = ScreenXToTick(_dragCurrentX);
            int newTick = Math.Max(0, (int)Math.Round(newTickD));

            if (newTick != _dragSourceTick && KeyframeService.Project != null)
            {
                KeyframeService.Project.PropertyKeyframes.MoveColumn(_dragSourceTick, newTick);
                KeyframeService.RaiseKeyframesChanged();
                KeyframeService.RaiseUndoSnapshot("Move keyframe");
            }
            InvalidateVisual();
            e.Handled = true;
        }

        void SeekToTick(int tick)
        {
            var proj = KeyframeService.Project;
            if (proj == null) return;
            proj.PausePlayback();
            proj.GoToTick(tick);
            // Select the matching row in the keyframe list.
            KeyframeService.RaiseTickSelected(tick);
        }

        // ---- Layout ----

        protected override Size MeasureOverride(Size availableSize)
            => new Size(availableSize.Width.IsFinite() ? availableSize.Width : 0,
                        DesiredHeight);

        const double DesiredHeight = 18.0;

        protected override Size ArrangeOverride(Size finalSize)
            => new Size(finalSize.Width, DesiredHeight);
    }

    static class DoubleExtensions
    {
        public static bool IsFinite(this double v) => !double.IsInfinity(v) && !double.IsNaN(v);
    }
}
