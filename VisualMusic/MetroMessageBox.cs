using MahApps.Metro.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VisualMusic
{
    /// <summary>
    /// Compact Metro-styled replacement for WPF MessageBox.Show calls.
    /// </summary>
    public static class MetroMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText)
            => Show(null, messageBoxText, Program.AppName, MessageBoxButton.OK, MessageBoxImage.None);

        public static MessageBoxResult Show(string messageBoxText, string caption)
            => Show(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);

        public static MessageBoxResult Show(string messageBoxText, string caption,
            MessageBoxButton button, MessageBoxImage icon)
            => Show(null, messageBoxText, caption, button, icon);

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
            MessageBoxButton button, MessageBoxImage icon)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                return dispatcher.Invoke(() => Show(owner, messageBoxText, caption, button, icon));
            }

            var resolvedOwner = ResolveOwner(owner);
            var dialog = new MetroMessageBoxWindow(
                messageBoxText,
                caption ?? Program.AppName,
                button,
                icon,
                resolvedOwner)
            {
                Owner = resolvedOwner
            };
            dialog.ShowDialog();
            return dialog.Result;
        }

        static Window ResolveOwner(Window owner)
        {
            if (owner != null) return owner;

            var app = Application.Current;
            return app?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                ?? app?.MainWindow
                ?? app?.Windows.OfType<Window>().FirstOrDefault();
        }

        sealed class MetroMessageBoxWindow : MetroWindow
        {
            readonly MessageBoxButton _buttons;
            Button _defaultButton;
            bool _hasExplicitResult;

            public MessageBoxResult Result { get; private set; }

            public MetroMessageBoxWindow(string message, string caption,
                MessageBoxButton buttons, MessageBoxImage icon, Window owner)
            {
                _buttons = buttons;
                Result = DefaultResult(buttons);

                Title = caption;
                MinWidth = 260;
                MaxWidth = owner != null ? System.Math.Max(360, owner.ActualWidth - 80) : 560;
                SizeToContent = SizeToContent.WidthAndHeight;
                ResizeMode = ResizeMode.NoResize;
                ShowInTaskbar = owner == null;
                WindowStartupLocation = owner != null
                    ? WindowStartupLocation.CenterOwner
                    : WindowStartupLocation.CenterScreen;

                Content = BuildContent(message, icon);
            }

            Grid BuildContent(string message, MessageBoxImage icon)
            {
                var root = new Grid { Margin = new Thickness(24, 20, 24, 18) };
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var body = new Grid();
                body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var (iconGlyph, iconBrush) = GetIcon(icon);
                if (!string.IsNullOrEmpty(iconGlyph))
                {
                    var iconBlock = new TextBlock
                    {
                        Text = iconGlyph,
                        Margin = new Thickness(0, 1, 16, 0),
                        FontFamily = SymbolFont,
                        FontSize = 28,
                        Foreground = iconBrush,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    Grid.SetColumn(iconBlock, 0);
                    body.Children.Add(iconBlock);
                }

                var messageBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    LineHeight = 20
                };

                var scroller = new ScrollViewer
                {
                    Content = messageBlock,
                    MaxHeight = 360,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                Grid.SetColumn(scroller, 1);
                body.Children.Add(scroller);

                Grid.SetRow(body, 0);
                root.Children.Add(body);

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 22, 0, 0)
                };

                foreach (var (text, result, isDefault, isCancel) in ButtonSpecs(_buttons))
                {
                    var button = new Button
                    {
                        Content = text,
                        MinWidth = 86,
                        Margin = new Thickness(8, 0, 0, 0),
                        IsDefault = isDefault,
                        IsCancel = isCancel
                    };
                    button.Click += (_, _) => CloseWith(result);
                    buttonPanel.Children.Add(button);
                    if (isDefault)
                        _defaultButton = button;
                }

                Grid.SetRow(buttonPanel, 1);
                root.Children.Add(buttonPanel);
                return root;
            }

            protected override void OnSourceInitialized(System.EventArgs e)
            {
                base.OnSourceInitialized(e);
                _defaultButton?.Focus();
            }

            void CloseWith(MessageBoxResult result)
            {
                _hasExplicitResult = true;
                Result = result;
                DialogResult = true;
                Close();
            }

            protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
            {
                if (!_hasExplicitResult)
                    Result = DefaultResult(_buttons);
                base.OnClosing(e);
            }

            static (string Text, MessageBoxResult Result, bool IsDefault, bool IsCancel)[] ButtonSpecs(
                MessageBoxButton buttons)
            {
                return buttons switch
                {
                    MessageBoxButton.OKCancel => new[]
                    {
                        ("OK", MessageBoxResult.OK, true, false),
                        ("Cancel", MessageBoxResult.Cancel, false, true)
                    },
                    MessageBoxButton.YesNo => new[]
                    {
                        ("Yes", MessageBoxResult.Yes, true, false),
                        ("No", MessageBoxResult.No, false, true)
                    },
                    MessageBoxButton.YesNoCancel => new[]
                    {
                        ("Yes", MessageBoxResult.Yes, true, false),
                        ("No", MessageBoxResult.No, false, false),
                        ("Cancel", MessageBoxResult.Cancel, false, true)
                    },
                    _ => new[]
                    {
                        ("OK", MessageBoxResult.OK, true, true)
                    },
                };
            }

            static MessageBoxResult DefaultResult(MessageBoxButton buttons)
            {
                return buttons switch
                {
                    MessageBoxButton.YesNo => MessageBoxResult.No,
                    MessageBoxButton.OKCancel => MessageBoxResult.Cancel,
                    MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
                    _ => MessageBoxResult.OK,
                };
            }

            static readonly FontFamily SymbolFont = new FontFamily("Segoe MDL2 Assets");

            static (string Glyph, Brush Brush) GetIcon(MessageBoxImage icon)
            {
                return icon switch
                {
                    // Glyphs from Segoe MDL2 Assets (present on Win10/11).
                    MessageBoxImage.Error => ("", Brush(0xE8, 0x11, 0x23)),      // red Error
                    MessageBoxImage.Warning => ("", Brush(0xF7, 0x9A, 0x1C)),    // amber Warning
                    MessageBoxImage.Question => ("", Brush(0x00, 0x78, 0xD4)),   // blue Help
                    MessageBoxImage.Information => ("", Brush(0x00, 0x78, 0xD4)),// blue Info
                    _ => (string.Empty, System.Windows.Media.Brushes.Transparent)
                };
            }

            static SolidColorBrush Brush(byte r, byte g, byte b)
            {
                var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                brush.Freeze();
                return brush;
            }
        }
    }
}