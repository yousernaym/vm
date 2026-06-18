using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VisualMusic.Controls
{
    /// <summary>
    /// Application preferences dialog.
    /// Currently exposes MahApps.Metro theme selection (base color + accent color scheme).
    /// Changes are previewed live; Cancel reverts the theme to what it was on open.
    /// </summary>
    public partial class PreferencesWindow : MetroWindow, INotifyPropertyChanged
    {
        // ---- INotifyPropertyChanged ----

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- Theme state ----

        readonly string _originalBase;
        readonly string _originalScheme;

        // ---- Bindable collections ----

        public IEnumerable<string> BaseColors => ThemeManager.Current.BaseColors;
        public IEnumerable<string> ColorSchemes => ThemeManager.Current.ColorSchemes;

        // ---- Bindable selections ----

        string _selectedBaseColor;
        public string SelectedBaseColor
        {
            get => _selectedBaseColor;
            set
            {
                if (_selectedBaseColor == value) return;
                _selectedBaseColor = value;
                Notify();
                ApplyTheme();
            }
        }

        string _selectedColorScheme;
        public string SelectedColorScheme
        {
            get => _selectedColorScheme;
            set
            {
                if (_selectedColorScheme == value) return;
                _selectedColorScheme = value;
                Notify();
                ApplyTheme();
            }
        }

        // ---- Constructor ----

        public PreferencesWindow()
        {
            DataContext = this;
            InitializeComponent();

            // Open the dropdowns on the currently-applied theme. DetectTheme reflects the live
            // theme; fall back to saved settings if detection fails.
            var current = ThemeManager.Current.DetectTheme(Application.Current);
            _originalBase = current?.BaseColorScheme ?? AppSettings.Instance.ThemeBaseColorOrDefault;
            _originalScheme = current?.ColorScheme ?? AppSettings.Instance.ThemeColorSchemeOrDefault;

            // Assign the backing fields directly (no ApplyTheme — the theme is already applied),
            // then notify so the bound ComboBoxes show the current selection.
            _selectedBaseColor = _originalBase;
            _selectedColorScheme = _originalScheme;
            Notify(nameof(SelectedBaseColor));
            Notify(nameof(SelectedColorScheme));
        }

        // ---- Helpers ----

        void ApplyTheme()
        {
            if (_selectedBaseColor == null || _selectedColorScheme == null) return;
            ThemeManager.Current.ChangeTheme(Application.Current, _selectedBaseColor, _selectedColorScheme);
        }

        // ---- Button handlers ----

        void Ok_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.ThemeBaseColor = _selectedBaseColor;
            AppSettings.Instance.ThemeColorScheme = _selectedColorScheme;
            AppSettings.Instance.Save();
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Revert to the theme that was active when the dialog opened.
            ThemeManager.Current.ChangeTheme(Application.Current, _originalBase, _originalScheme);
            DialogResult = false;
        }
    }
}
