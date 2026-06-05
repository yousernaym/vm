using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using VisualMusic.Controls;
using static VisualMusic.Keyframes.KeyframeService;

namespace VisualMusic.Keyframes
{
    // =========================================================================
    // KeyframeAdorner — thin colored border drawn over a keyframeable control
    // =========================================================================

    internal sealed class KeyframeAdorner : Adorner
    {
        static readonly Brush FillGreen  = Freeze(new SolidColorBrush(Color.FromArgb(45, 0, 230, 80)));
        static readonly Brush FillBlue   = Freeze(new SolidColorBrush(Color.FromArgb(45, 60, 140, 255)));
        static readonly Pen   PenGreen   = FreezePen(new SolidColorBrush(Color.FromArgb(200, 0, 200, 70)),  1.5);
        static readonly Pen   PenBlue    = FreezePen(new SolidColorBrush(Color.FromArgb(200, 60, 130, 255)), 1.5);

        static T Freeze<T>(T obj) where T : Freezable { obj.Freeze(); return obj; }
        static Pen FreezePen(Brush b, double t) { var p = new Pen(b, t); p.Freeze(); return p; }

        public enum State { Green, Blue }
        public State Current { get; set; }

        public KeyframeAdorner(UIElement element, State state) : base(element)
        {
            Current = state;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var fe  = (FrameworkElement)AdornedElement;
            var rect = new Rect(0, 0, fe.ActualWidth, fe.ActualHeight);
            if (Current == State.Green)
                dc.DrawRectangle(FillGreen, PenGreen, rect);
            else
                dc.DrawRectangle(FillBlue, PenBlue, rect);
        }
    }

    // =========================================================================
    // Keyframing — attached behavior applied in XAML to keyframeable controls
    // =========================================================================

    /// <summary>
    /// Attached behaviour that wires a WPF control to the per-property keyframe model.
    /// Set <see cref="PropertyIdProperty"/> (required), <see cref="ScopeProperty"/>,
    /// <see cref="DisplayNameProperty"/>, and <see cref="InterpolatableProperty"/> in XAML
    /// to activate the behaviour.
    /// </summary>
    public static class Keyframing
    {
        // ---- Attached dependency properties ----

        public static readonly DependencyProperty PropertyIdProperty =
            DependencyProperty.RegisterAttached("PropertyId", typeof(string), typeof(Keyframing),
                new PropertyMetadata(null, OnPropertyIdChanged));

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.RegisterAttached("DisplayName", typeof(string), typeof(Keyframing),
                new PropertyMetadata(""));

        public static readonly DependencyProperty ScopeProperty =
            DependencyProperty.RegisterAttached("Scope", typeof(KfScope), typeof(Keyframing),
                new PropertyMetadata(KfScope.Project));

        public static readonly DependencyProperty InterpolatableProperty =
            DependencyProperty.RegisterAttached("Interpolatable", typeof(bool), typeof(Keyframing),
                new PropertyMetadata(true));

        // Getters / setters required by XAML
        public static string  GetPropertyId  (DependencyObject d) => (string) d.GetValue(PropertyIdProperty);
        public static void    SetPropertyId  (DependencyObject d, string v)  => d.SetValue(PropertyIdProperty,  v);
        public static string  GetDisplayName (DependencyObject d) => (string) d.GetValue(DisplayNameProperty);
        public static void    SetDisplayName (DependencyObject d, string v)  => d.SetValue(DisplayNameProperty, v);
        public static KfScope GetScope       (DependencyObject d) => (KfScope)d.GetValue(ScopeProperty);
        public static void    SetScope       (DependencyObject d, KfScope v) => d.SetValue(ScopeProperty,       v);
        public static bool    GetInterpolatable(DependencyObject d) => (bool) d.GetValue(InterpolatableProperty);
        public static void    SetInterpolatable(DependencyObject d, bool v) => d.SetValue(InterpolatableProperty, v);

        // ---- Per-element runtime state ----

        class ElementState
        {
            public KeyframeAdorner Adorner;
            /// <summary>Value cached when the control is green or default (not blue).</summary>
            public object SafeValue;
            /// <summary>Blocks re-entrant interception during revert.</summary>
            public bool Reverting;
            /// <summary>True once event handlers have been attached (guards repeated Loaded events).</summary>
            public bool Initialized;
        }

        static readonly ConditionalWeakTable<FrameworkElement, ElementState> _states = new();

        // ---- Activation ----

        static void OnPropertyIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe) return;
            if (e.NewValue is not string propId || string.IsNullOrEmpty(propId)) return;

            fe.Loaded   += OnLoaded;
            fe.Unloaded += OnUnloaded;

            // If already loaded, run setup now (handles XAML DataTemplate recycling)
            if (fe.IsLoaded) Setup(fe);
        }

        static void OnLoaded(object sender, RoutedEventArgs e) => Setup((FrameworkElement)sender);
        static void OnUnloaded(object sender, RoutedEventArgs e) => Teardown((FrameworkElement)sender);

        static void Setup(FrameworkElement fe)
        {
            var state = _states.GetOrCreateValue(fe);

            // Guard: Loaded fires every time the element re-enters the visual tree (panel toggles).
            // Without this guard, handlers (and the edit-interception prompt) accumulate, causing the
            // "Create keyframe?" dialog to appear once per accumulated subscription.
            if (state.Initialized) { Refresh(fe); return; }
            state.Initialized = true;

            // Register the friendly name so menus elsewhere (e.g. the keyframe list) can label this property.
            KeyframeService.RegisterDisplayName(GetPropertyId(fe), GetDisplayName(fe));

            KeyframeService.RefreshRequested  += () => Refresh(fe);
            KeyframeService.KeyframesChanged  += () => Refresh(fe);

            AttachContextMenu(fe);
            AttachEditInterception(fe);
            Refresh(fe);
        }

        static void Teardown(FrameworkElement fe)
        {
            if (_states.TryGetValue(fe, out var state) && state.Adorner != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(fe);
                layer?.Remove(state.Adorner);
                state.Adorner = null;
            }
            // Note: event subscriptions via lambda can't be cleanly removed without stored delegates;
            // the WeakReference pattern would be needed for a production build.  Since elements are
            // typically long-lived (not re-created in a DataTemplate), this is acceptable for now.
        }

        // ---- Coloring ----

        static void Refresh(FrameworkElement fe)
        {
            var propId       = GetPropertyId(fe);
            var scope        = GetScope(fe);
            if (string.IsNullOrEmpty(propId)) return;

            bool green = KeyframeService.HasKeyHereForAll(propId, scope);
            bool blue  = !green && KeyframeService.HasAnyKeyForAny(propId, scope);

            var state = _states.GetOrCreateValue(fe);

            if (!blue && !state.Reverting)
                CacheValue(fe, state);

            // Update adorner
            var layer = AdornerLayer.GetAdornerLayer(fe);
            if (layer == null) return;

            if (!green && !blue)
            {
                if (state.Adorner != null) { layer.Remove(state.Adorner); state.Adorner = null; }
                return;
            }

            var desired = green ? KeyframeAdorner.State.Green : KeyframeAdorner.State.Blue;
            if (state.Adorner == null)
            {
                state.Adorner = new KeyframeAdorner(fe, desired);
                layer.Add(state.Adorner);
            }
            else if (state.Adorner.Current != desired)
            {
                state.Adorner.Current = desired;
                state.Adorner.InvalidateVisual();
            }
        }

        // ---- Value caching (for blue-control revert) ----

        static void CacheValue(FrameworkElement fe, ElementState state)
        {
            state.SafeValue = fe switch
            {
                TbSliderWpf s => s.Value,
                CheckBox    c => c.IsChecked,
                ComboBox    cb=> cb.SelectedIndex,
                HueSatButtonWpf h => h.SelectedXnaColor,
                _ => null
            };
        }

        // ---- Edit interception ----

        static void AttachEditInterception(FrameworkElement fe)
        {
            switch (fe)
            {
                case TbSliderWpf slider:
                    slider.CommitChanges += (_, _) => OnCommit(slider);
                    break;
                case CheckBox cb:
                    cb.Click += (_, _) => OnCommit(cb);
                    break;
                case ComboBox combo:
                    combo.SelectionChanged += (_, _) => OnCommit(combo);
                    break;
                case HueSatButtonWpf hue:
                    hue.SelectionChanged += (_, _) => OnCommit(hue);
                    break;
            }
        }

        static void OnCommit(FrameworkElement fe)
        {
            var propId = GetPropertyId(fe);
            var scope  = GetScope(fe);
            if (string.IsNullOrEmpty(propId)) return;

            var state = _states.GetOrCreateValue(fe);
            if (state.Reverting) return;

            // Editing a keyframeable control stops playback so the edit lands at a stable position.
            KeyframeService.PausePlayback();

            // Shared gate: proceeds for green / un-keyframed controls; prompts for blue ones.
            if (!KeyframeService.EnsureKeyframeForEdit(propId, scope))
            {
                // User declined → revert to the safe (pre-edit) value.
                state.Reverting = true;
                try { RevertValue(fe, state); }
                finally { state.Reverting = false; }
            }
        }

        static void RevertValue(FrameworkElement fe, ElementState state)
        {
            if (state.SafeValue == null) return;
            switch (fe)
            {
                case TbSliderWpf s  when state.SafeValue is double d:   s.Value          = d;   break;
                case CheckBox    c  when state.SafeValue is bool   b:   c.IsChecked      = b;   break;
                case ComboBox    cb when state.SafeValue is int    i:   cb.SelectedIndex = i;   break;
                case HueSatButtonWpf h:
                    h.SelectedXnaColor = (Microsoft.Xna.Framework.Color?)state.SafeValue;       break;
            }
        }

        // ---- Context menu ----

        static void AttachContextMenu(FrameworkElement fe)
        {
            var menu = new ContextMenu();
            fe.ContextMenu = menu;
            fe.ContextMenuOpening += (_, _) =>
            {
                // Right-clicking a keyframeable control stops playback before showing the menu.
                KeyframeService.PausePlayback();
                RebuildMenu(fe, menu);
            };
        }

        static void RebuildMenu(FrameworkElement fe, ContextMenu menu)
        {
            var propId        = GetPropertyId(fe);
            var scope         = GetScope(fe);
            var displayName   = GetDisplayName(fe);
            var interpolatable= GetInterpolatable(fe);
            if (string.IsNullOrEmpty(propId)) return;

            bool hasHere = KeyframeService.HasKeyHereForAll(propId, scope);
            bool hasAny  = KeyframeService.HasAnyKeyForAny(propId, scope);
            bool hasPrev = false, hasNext = false;

            if (KeyframeService.Project != null)
            {
                var kfs = KeyframeService.Project.PropertyKeyframes;
                int tick = KeyframeService.CurrentTick;
                foreach (var id in KeyframeService.ResolveIds(propId, scope))
                {
                    if (kfs.PrevTickForProperty(id, tick).HasValue) hasPrev = true;
                    if (kfs.NextTickForProperty(id, tick).HasValue) hasNext = true;
                }
            }

            menu.Items.Clear();

            // Add / Remove
            var addItem = new MenuItem { Header = "_Add keyframe at playback position",    IsEnabled = !hasHere };
            var remItem = new MenuItem { Header = "_Remove keyframe at playback position",  IsEnabled = hasHere  };
            addItem.Click += (_, _) => { KeyframeService.AddKey(propId, scope);    };
            remItem.Click += (_, _) => { KeyframeService.RemoveKey(propId, scope); };
            menu.Items.Add(addItem);
            menu.Items.Add(remItem);

            menu.Items.Add(new Separator());

            // Next / Prev
            var prevItem = new MenuItem { Header = "Go to _previous keyframe", IsEnabled = hasPrev };
            var nextItem = new MenuItem { Header = "Go to _next keyframe",     IsEnabled = hasNext };
            prevItem.Click += (_, _) => KeyframeService.GoToPrev(propId, scope);
            nextItem.Click += (_, _) => KeyframeService.GoToNext(propId, scope);
            menu.Items.Add(prevItem);
            menu.Items.Add(nextItem);

            // Interpolation submenu (only for interpolatable controls)
            if (interpolatable)
            {
                menu.Items.Add(new Separator());
                var interpMenu = new MenuItem { Header = "_Interpolation", IsEnabled = hasHere };
                var curInterp  = hasHere ? KeyframeService.GetInterpolation(propId, scope) : null;

                foreach (var (label, mode) in new[]
                {
                    ("_Smooth (ease in/out)", KfInterpolation.Smooth),
                    ("_Linear",               KfInterpolation.Linear),
                    ("_Hold (step)",          KfInterpolation.Hold  ),
                })
                {
                    var capturedMode = mode;
                    var modeItem = new MenuItem
                    {
                        Header      = label,
                        IsCheckable = true,
                        IsChecked   = curInterp == mode,
                    };
                    modeItem.Click += (_, _) => KeyframeService.SetInterpolation(propId, scope, capturedMode);
                    interpMenu.Items.Add(modeItem);
                }
                menu.Items.Add(interpMenu);
            }
        }
    }
}
