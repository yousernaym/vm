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
            bool _hasExplicitResult;

            public MessageBoxResult Result { get; private set; }

            public MetroMessageBoxWindow(string message, string caption,
                MessageBoxButton buttons, MessageBoxImage icon, Window owner)
            {
                _buttons = buttons;
                Result = DefaultResult(buttons);

                Title = caption;
                Width = 520;
                MinWidth = 360;
                MaxWidth = owner != null ? System.Math.Max(360, owner.ActualWidth - 80) : 720;
                SizeToContent = SizeToContent.Height;
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

                var iconText = GetIconText(icon);
                if (!string.IsNullOrEmpty(iconText))
                {
                    var iconBlock = new TextBlock
                    {
                        Text = iconText,
                        Width = 32,
                        Height = 32,
                        Margin = new Thickness(0, 1, 14, 0),
                        FontSize = 22,
                        FontWeight = FontWeights.SemiBold,
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
                }

                Grid.SetRow(buttonPanel, 1);
                root.Children.Add(buttonPanel);
                return root;
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

            static string GetIconText(MessageBoxImage icon)
            {
                return icon switch
                {
                    MessageBoxImage.Error => "X",
                    MessageBoxImage.Warning => "!",
                    MessageBoxImage.Question => "?",
                    MessageBoxImage.Information => "i",
                    _ => ""
                };
            }
        }
    }
}