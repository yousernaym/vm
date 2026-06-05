using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisualMusic.Controls
{
    public partial class TbSliderWpf : UserControl
    {
        bool _updating;

        public TbSliderWpf()
        {
            InitializeComponent();
        }

        // ---- DependencyProperties ----

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(double?), typeof(TbSliderWpf),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged));

        public double? Value
        {
            get => (double?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (TbSliderWpf)d;
            if (!ctrl._updating)
                ctrl.RefreshFromValue();
        }

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register(
            nameof(Min), typeof(double), typeof(TbSliderWpf),
            new PropertyMetadata(0.0, OnRangeChanged));

        public double Min
        {
            get => (double)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(
            nameof(Max), typeof(double), typeof(TbSliderWpf),
            new PropertyMetadata(100.0, OnRangeChanged));

        public double Max
        {
            get => (double)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((TbSliderWpf)d).RefreshFromValue();

        public static readonly DependencyProperty DecimalsProperty = DependencyProperty.Register(
            nameof(Decimals), typeof(int), typeof(TbSliderWpf),
            new PropertyMetadata(2, OnRangeChanged));

        public int Decimals
        {
            get => (int)GetValue(DecimalsProperty);
            set => SetValue(DecimalsProperty, value);
        }

        public static readonly DependencyProperty Decimals2Property = DependencyProperty.Register(
            nameof(Decimals2), typeof(int), typeof(TbSliderWpf),
            new PropertyMetadata(2));

        /// <summary>Display decimals used when ExpBase >= 0 (exponential mode).</summary>
        public int Decimals2
        {
            get => (int)GetValue(Decimals2Property);
            set => SetValue(Decimals2Property, value);
        }

        public static readonly DependencyProperty ExpBaseProperty = DependencyProperty.Register(
            nameof(ExpBase), typeof(double), typeof(TbSliderWpf),
            new PropertyMetadata(-1.0));

        /// <summary>Base for exponential mapping. -1 = linear mode.</summary>
        public double ExpBase
        {
            get => (double)GetValue(ExpBaseProperty);
            set => SetValue(ExpBaseProperty, value);
        }

        // ---- Events ----

        public event EventHandler ValueChanged;
        public event EventHandler CommitChanges;

        // ---- Helpers ----

        bool IsExponential => ExpBase >= 0;
        int DisplayDecimals => IsExponential ? Decimals2 : Decimals;

        /// <summary>Convert from model value to slider position.</summary>
        double ModelToSlider(double modelVal)
            => IsExponential ? Math.Log(modelVal, ExpBase) : modelVal;

        /// <summary>Convert from slider position to model value.</summary>
        double SliderToModel(double sliderPos)
            => IsExponential ? Math.Pow(ExpBase, sliderPos) : sliderPos;

        void RefreshFromValue()
        {
            if (_updating) return;
            _updating = true;
            try
            {
                if (Value == null)
                {
                    textBox.Text = "";
                    slider.Value = Min;
                }
                else
                {
                    double mv = Value.Value;
                    textBox.Text = mv.ToString("f" + DisplayDecimals);
                    double sliderVal = ModelToSlider(mv);
                    if (double.IsFinite(sliderVal))
                        slider.Value = Math.Clamp(sliderVal, Min, Max);
                }
            }
            finally { _updating = false; }
        }

        // ---- Slider events ----

        void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updating) return;
            _updating = true;
            try
            {
                double model = SliderToModel(slider.Value);
                textBox.Text = model.ToString("f" + DisplayDecimals);
                Value = model;
            }
            finally { _updating = false; }
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Only the left button commits a value edit; right-click is reserved for the context menu.
            if (e.ChangedButton == MouseButton.Left)
                CommitChanges?.Invoke(this, EventArgs.Empty);
        }

        void Slider_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CommitChanges?.Invoke(this, EventArgs.Empty);
        }

        // ---- TextBox events ----

        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updating) return;
            if (!double.TryParse(textBox.Text, out double parsed)) return;
            _updating = true;
            try
            {
                Value = parsed;
                double sliderVal = ModelToSlider(parsed);
                if (double.IsFinite(sliderVal))
                    slider.Value = Math.Clamp(sliderVal, Min, Max);
            }
            finally { _updating = false; }
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CommitChanges?.Invoke(this, EventArgs.Empty);
        }

        void TextBox_LostFocus(object sender, RoutedEventArgs e)
            => CommitChanges?.Invoke(this, EventArgs.Empty);
    }
}
