using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Common.Controls;

[TemplatePart(Name = "PART_SpectrumCanvas", Type = typeof(Canvas))]
public class SpectrumAnalyzerControl : Control
{
    #region Fields
    private readonly DispatcherTimer animationTimer;
    private Canvas spectrumCanvas;
    private ISpectrumPlayer soundPlayer;
    private readonly List<Shape> barShapes = new List<Shape>();
    private readonly List<Shape> peakShapes = new List<Shape>();
    private double[] barHeights;
    private double[] peakHeights;
    private float[] channelData = new float[2048];
    private float[] channelPeakData;
    private double bandWidth = 1.0;
    private double barWidth = 1;
    private int maximumFrequencyIndex = 2047;
    private int minimumFrequencyIndex;
    private int[] barIndexMax;
    private int[] barLogScaleIndexMax;
    #endregion

    #region Constants
    private const int scaleFactorLinear = 9;
    private const int scaleFactorSqr = 2;
    private const double minDBValue = -90;
    private const double maxDBValue = 0;
    private const double dbScale = (maxDBValue - minDBValue);
    #endregion

    #region Dependency Properties
    public ISpectrumPlayer RegisterSoundPlayer
    {
        get { return base.GetValue(IsPlayingProperty) as ISpectrumPlayer; }
        set { base.SetValue(IsPlayingProperty, value); }
    }

    public static readonly DependencyProperty RegisterSoundPlayerProperty =
      DependencyProperty.Register("RegisterSoundPlayer", typeof(ISpectrumPlayer), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(null, OnRegisterSoundPlayerChanged));

    private static void OnRegisterSoundPlayerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;

        spectrumAnalyzerControl.soundPlayer = (ISpectrumPlayer)e.NewValue;
        spectrumAnalyzerControl.UpdateBarLayout();
        spectrumAnalyzerControl.animationTimer.Start();
    }

    public bool IsPlaying
    {
        get { return (bool)base.GetValue(IsPlayingProperty); }
        set { base.SetValue(IsPlayingProperty, value); }
    }

    public static readonly DependencyProperty IsPlayingProperty =
      DependencyProperty.Register("IsPlaying", typeof(bool), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(false, OnIsPlayingChanged));

    private static void OnIsPlayingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;

        if ((bool)e.NewValue && !spectrumAnalyzerControl.animationTimer.IsEnabled)
            spectrumAnalyzerControl.animationTimer.Start();
    }

    #region MaximumFrequency
    public static readonly DependencyProperty MaximumFrequencyProperty = DependencyProperty.Register("MaximumFrequency", typeof(int), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(20000, OnMaximumFrequencyChanged, OnCoerceMaximumFrequency));

    private static object OnCoerceMaximumFrequency(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceMaximumFrequency((int)value);
        else
            return value;
    }

    private static void OnMaximumFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnMaximumFrequencyChanged((int)e.OldValue, (int)e.NewValue);
    }

    protected virtual int OnCoerceMaximumFrequency(int value)
    {
        if ((int)value < MinimumFrequency)
            return MinimumFrequency + 1;
        return value;
    }

    protected virtual void OnMaximumFrequencyChanged(int oldValue, int newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// The maximum display frequency (right side) for the spectrum analyzer.
    /// </summary>
    /// <remarks>In usual practice, this value should be somewhere between 0 and half of the maximum sample rate. If using
    /// the maximum sample rate, this would be roughly 22000.</remarks>
    [Category("Common")]
    public int MaximumFrequency
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (int)GetValue(MaximumFrequencyProperty);
        }
        set
        {
            SetValue(MaximumFrequencyProperty, value);
        }
    }
    #endregion

    #region Minimum Frequency
    public static readonly DependencyProperty MinimumFrequencyProperty = DependencyProperty.Register("MinimumFrequency", typeof(int), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(20, OnMinimumFrequencyChanged, OnCoerceMinimumFrequency));

    private static object OnCoerceMinimumFrequency(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceMinimumFrequency((int)value);
        else
            return value;
    }

    private static void OnMinimumFrequencyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnMinimumFrequencyChanged((int)e.OldValue, (int)e.NewValue);
    }

    protected virtual int OnCoerceMinimumFrequency(int value)
    {
        if (value < 0)
            return value = 0;
        CoerceValue(MaximumFrequencyProperty);
        return value;
    }

    protected virtual void OnMinimumFrequencyChanged(int oldValue, int newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// The minimum display frequency (left side) for the spectrum analyzer.
    /// </summary>
    [Category("Common")]
    public int MinimumFrequency
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (int)GetValue(MinimumFrequencyProperty);
        }
        set
        {
            SetValue(MinimumFrequencyProperty, value);
        }
    }

    #endregion

    #region BarCount
    public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register("BarCount", typeof(int), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(32, OnBarCountChanged, OnCoerceBarCount));

    private static object OnCoerceBarCount(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceBarCount((int)value);
        else
            return value;
    }

    private static void OnBarCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnBarCountChanged((int)e.OldValue, (int)e.NewValue);
    }

    protected virtual int OnCoerceBarCount(int value)
    {
        value = Math.Max(value, 1);
        return value;
    }

    protected virtual void OnBarCountChanged(int oldValue, int newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// The number of bars to show on the sprectrum analyzer.
    /// </summary>
    /// <remarks>A bar's width can be a minimum of 1 pixel. If the BarSpacing and BarCount property result
    /// in the bars being wider than the chart itself, the BarCount will automatically scale down.</remarks>
    [Category("Common")]
    public int BarCount
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (int)GetValue(BarCountProperty);
        }
        set
        {
            SetValue(BarCountProperty, value);
        }
    }
    #endregion

    #region BarSpacing
    public static readonly DependencyProperty BarSpacingProperty = DependencyProperty.Register("BarSpacing", typeof(double), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(5.0d, OnBarSpacingChanged, OnCoerceBarSpacing));

    private static object OnCoerceBarSpacing(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceBarSpacing((double)value);
        else
            return value;
    }

    private static void OnBarSpacingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnBarSpacingChanged((double)e.OldValue, (double)e.NewValue);
    }

    protected virtual double OnCoerceBarSpacing(double value)
    {
        value = Math.Max(value, 0);
        return value;
    }

    protected virtual void OnBarSpacingChanged(double oldValue, double newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// The spacing, in pixels, between the bars.
    /// </summary>
    [Category("Common")]
    public double BarSpacing
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (double)GetValue(BarSpacingProperty);
        }
        set
        {
            SetValue(BarSpacingProperty, value);
        }
    }
    #endregion

    #region PeakFallDelay
    public static readonly DependencyProperty PeakFallDelayProperty = DependencyProperty.Register("PeakFallDelay", typeof(int), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(10, OnPeakFallDelayChanged, OnCoercePeakFallDelay));

    private static object OnCoercePeakFallDelay(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoercePeakFallDelay((int)value);
        else
            return value;
    }

    private static void OnPeakFallDelayChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnPeakFallDelayChanged((int)e.OldValue, (int)e.NewValue);
    }

    protected virtual int OnCoercePeakFallDelay(int value)
    {
        value = Math.Max(value, 0);
        return value;
    }

    protected virtual void OnPeakFallDelayChanged(int oldValue, int newValue)
    {

    }

    /// <summary>
    /// The delay factor for the peaks falling. This is relative to the
    /// refresh rate of the chart.
    /// </summary>
    [Category("Common")]
    public int PeakFallDelay
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (int)GetValue(PeakFallDelayProperty);
        }
        set
        {
            SetValue(PeakFallDelayProperty, value);
        }
    }
    #endregion

    #region IsFrequencyScaleLinear
    public static readonly DependencyProperty IsFrequencyScaleLinearProperty = DependencyProperty.Register("IsFrequencyScaleLinear", typeof(bool), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(false, OnIsFrequencyScaleLinearChanged, OnCoerceIsFrequencyScaleLinear));

    private static object OnCoerceIsFrequencyScaleLinear(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceIsFrequencyScaleLinear((bool)value);
        else
            return value;
    }

    private static void OnIsFrequencyScaleLinearChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnIsFrequencyScaleLinearChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    protected virtual bool OnCoerceIsFrequencyScaleLinear(bool value)
    {
        return value;
    }

    protected virtual void OnIsFrequencyScaleLinearChanged(bool oldValue, bool newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// If true, the bars will represent frequency buckets on a linear scale (making them all
    /// have equal band widths on the frequency scale). Otherwise, the bars will be layed out
    /// on a logrithmic scale, with each bar having a larger bandwidth than the one previous.
    /// </summary>
    [Category("Common")]
    public bool IsFrequencyScaleLinear
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (bool)GetValue(IsFrequencyScaleLinearProperty);
        }
        set
        {
            SetValue(IsFrequencyScaleLinearProperty, value);
        }
    }
    #endregion

    #region BarHeightScaling
    public static readonly DependencyProperty BarHeightScalingProperty = DependencyProperty.Register("BarHeightScaling", typeof(BarHeightScalingStyles), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(BarHeightScalingStyles.Decibel, OnBarHeightScalingChanged, OnCoerceBarHeightScaling));

    private static object OnCoerceBarHeightScaling(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceBarHeightScaling((BarHeightScalingStyles)value);
        else
            return value;
    }

    private static void OnBarHeightScalingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnBarHeightScalingChanged((BarHeightScalingStyles)e.OldValue, (BarHeightScalingStyles)e.NewValue);
    }

    protected virtual BarHeightScalingStyles OnCoerceBarHeightScaling(BarHeightScalingStyles value)
    {
        return value;
    }

    protected virtual void OnBarHeightScalingChanged(BarHeightScalingStyles oldValue, BarHeightScalingStyles newValue)
    {

    }

    /// <summary>
    /// If true, the bar height will be displayed linearly with the intensity value.
    /// Otherwise, the bars will be scaled with a square root function.
    /// </summary>
    [Category("Common")]
    public BarHeightScalingStyles BarHeightScaling
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (BarHeightScalingStyles)GetValue(BarHeightScalingProperty);
        }
        set
        {
            SetValue(BarHeightScalingProperty, value);
        }
    }
    #endregion

    #region AveragePeaks
    public static readonly DependencyProperty AveragePeaksProperty = DependencyProperty.Register("AveragePeaks", typeof(bool), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(false, OnAveragePeaksChanged, OnCoerceAveragePeaks));

    private static object OnCoerceAveragePeaks(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceAveragePeaks((bool)value);
        else
            return value;
    }

    private static void OnAveragePeaksChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnAveragePeaksChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    protected virtual bool OnCoerceAveragePeaks(bool value)
    {
        return value;
    }

    protected virtual void OnAveragePeaksChanged(bool oldValue, bool newValue)
    {

    }

    /// <summary>
    /// If true, each bar's peak value will be averaged with the previous
    /// bar's peak. This creates a smoothing effect on the bars.
    /// </summary>
    [Category("Common")]
    public bool AveragePeaks
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (bool)GetValue(AveragePeaksProperty);
        }
        set
        {
            SetValue(AveragePeaksProperty, value);
        }
    }
    #endregion

    #region BarStyle
    public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register("BarStyle", typeof(Style), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBarStyleChanged), new CoerceValueCallback(OnCoerceBarStyle)));

    private static object OnCoerceBarStyle(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceBarStyle((Style)value);
        else
            return value;
    }

    private static void OnBarStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnBarStyleChanged((Style)e.OldValue, (Style)e.NewValue);
    }

    protected virtual Style OnCoerceBarStyle(Style value)
    {
        return value;
    }

    protected virtual void OnBarStyleChanged(Style oldValue, Style newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// A style with which to draw the bars on the spectrum analyzer.
    /// </summary>
    public Style BarStyle
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (Style)GetValue(BarStyleProperty);
        }
        set
        {
            SetValue(BarStyleProperty, value);
        }
    }
    #endregion

    #region PeakStyle
    public static readonly DependencyProperty PeakStyleProperty = DependencyProperty.Register("PeakStyle", typeof(Style), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(null, new PropertyChangedCallback(OnPeakStyleChanged), new CoerceValueCallback(OnCoercePeakStyle)));

    private static object OnCoercePeakStyle(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoercePeakStyle((Style)value);
        else
            return value;
    }

    private static void OnPeakStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnPeakStyleChanged((Style)e.OldValue, (Style)e.NewValue);
    }

    protected virtual Style OnCoercePeakStyle(Style value)
    {

        return value;
    }

    protected virtual void OnPeakStyleChanged(Style oldValue, Style newValue)
    {
        UpdateBarLayout();
    }

    /// <summary>
    /// A style with which to draw the falling peaks on the spectrum analyzer.
    /// </summary>
    public Style PeakStyle
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (Style)GetValue(PeakStyleProperty);
        }
        set
        {
            SetValue(PeakStyleProperty, value);
        }
    }
    #endregion

    #region ActualBarWidth
    public static readonly DependencyProperty ActualBarWidthProperty = DependencyProperty.Register("ActualBarWidth", typeof(double), typeof(SpectrumAnalyzerControl), new UIPropertyMetadata(0.0d, new PropertyChangedCallback(OnActualBarWidthChanged), new CoerceValueCallback(OnCoerceActualBarWidth)));

    private static object OnCoerceActualBarWidth(DependencyObject o, object value)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            return spectrumAnalyzerControl.OnCoerceActualBarWidth((double)value);
        else
            return value;
    }

    private static void OnActualBarWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        SpectrumAnalyzerControl spectrumAnalyzerControl = o as SpectrumAnalyzerControl;
        if (spectrumAnalyzerControl != null)
            spectrumAnalyzerControl.OnActualBarWidthChanged((double)e.OldValue, (double)e.NewValue);
    }

    protected virtual double OnCoerceActualBarWidth(double value)
    {
        return value;
    }

    protected virtual void OnActualBarWidthChanged(double oldValue, double newValue)
    {

    }

    /// <summary>
    /// The actual width that the bars will be drawn at.
    /// </summary>
    public double ActualBarWidth
    {
        // IMPORTANT: To maintain parity between setting a property in XAML and procedural code, do not touch the getter and setter inside this dependency property!
        get
        {
            return (double)GetValue(ActualBarWidthProperty);
        }
        protected set
        {
            SetValue(ActualBarWidthProperty, value);
        }
    }

    #endregion
    #endregion

    #region Enums
    /// <summary>
    /// The different ways that the bar height can be scaled by the spectrum analyzer.
    /// </summary>
    public enum BarHeightScalingStyles
    {
        /// <summary>
        /// A decibel scale. Formula: 20 * Log10(FFTValue). Total bar height
        /// is scaled from -90 to 0 dB.
        /// </summary>
        Decibel,

        /// <summary>
        /// A non-linear squareroot scale. Formula: Sqrt(FFTValue) * 2 * BarHeight.
        /// </summary>
        Sqrt,

        /// <summary>
        /// A linear scale. Formula: 9 * FFTValue * BarHeight.
        /// </summary>
        Linear
    }

    /// <summary>
    /// The styles that the spectrum analyzer can draw the bars.
    /// </summary>
    public enum BarDrawingStyles
    {
        /// <summary>
        /// Square bars.
        /// </summary>
        Square,

        /// <summary>
        /// Rounded bars with the corner radius as half the
        /// bar width.
        /// </summary>
        Rounded
    }
    #endregion

    #region Template Overrides
    public override void OnApplyTemplate()
    {
        spectrumCanvas = GetTemplateChild("PART_SpectrumCanvas") as Canvas;
        UpdateBarLayout();
    }
    #endregion

    #region Constructors
    static SpectrumAnalyzerControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumAnalyzerControl), new FrameworkPropertyMetadata(typeof(SpectrumAnalyzerControl)));
    }

    public SpectrumAnalyzerControl()
    {
        animationTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
        {
            Interval = TimeSpan.FromMilliseconds(25),
        };
        animationTimer.Tick += animationTimer_Tick;
    }
    #endregion

    #region Public Methods
    //
    #endregion

    #region Event Overrides
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        UpdateBarLayout();
        UpdateSpectrum();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateBarLayout();
        UpdateSpectrum();
    }
    #endregion

    #region Private Drawing Methods
    private void UpdateSpectrum()
    {
        if (soundPlayer == null || spectrumCanvas == null || spectrumCanvas.RenderSize.Width < 1 || spectrumCanvas.RenderSize.Height < 1)
            return;

        if (IsPlaying && (soundPlayer.GetFFTData(channelData) < 1))
            return;

        UpdateSpectrumShapes();
    }


    private void UpdateSpectrumShapes()
    {
        bool allZero = true;
        double fftBucketHeight = 0f;
        double barHeight = 0f;
        double lastPeakHeight = 0f;
        double peakYPos = 0f;
        double height = spectrumCanvas.RenderSize.Height;
        int barIndex = 0;
        double peakDotHeight = Math.Max(barWidth / 2.0f, 1);
        double barHeightScale = (height - peakDotHeight);

        for (int i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++)
        {
            // If we're paused, keep drawing, but set the current height to 0 so the peaks fall.
            if (!IsPlaying)
            {
                barHeight = 0f;
            }
            else // Draw the maximum value for the bar's band
            {
                switch (BarHeightScaling)
                {
                    case BarHeightScalingStyles.Decibel:
                        double dbValue = 20 * Math.Log10((double)channelData[i]);
                        fftBucketHeight = ((dbValue - minDBValue) / dbScale) * barHeightScale;
                        break;
                    case BarHeightScalingStyles.Linear:
                        fftBucketHeight = (channelData[i] * scaleFactorLinear) * barHeightScale;
                        break;
                    case BarHeightScalingStyles.Sqrt:
                        fftBucketHeight = (((Math.Sqrt((double)channelData[i])) * scaleFactorSqr) * barHeightScale);
                        break;
                }

                if (barHeight < fftBucketHeight)
                    barHeight = fftBucketHeight;
                if (barHeight < 0f)
                    barHeight = 0f;
            }

            // If this is the last FFT bucket in the bar's group, draw the bar.
            int currentIndexMax = IsFrequencyScaleLinear ? barIndexMax[barIndex] : barLogScaleIndexMax[barIndex];
            if (i == currentIndexMax)
            {
                // Peaks can't surpass the height of the control.
                if (barHeight > height)
                    barHeight = height;

                if (AveragePeaks && barIndex > 0)
                    barHeight = (lastPeakHeight + barHeight) / 2;

                peakYPos = barHeight;

                if (channelPeakData[barIndex] < peakYPos)
                    channelPeakData[barIndex] = (float)peakYPos;
                else
                    channelPeakData[barIndex] = (float)(peakYPos + (PeakFallDelay * channelPeakData[barIndex])) / ((float)(PeakFallDelay + 1));

                double xCoord = BarSpacing + (barWidth * barIndex) + (BarSpacing * barIndex) + 1;

                barShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - barHeight, 0, 0);
                barShapes[barIndex].Height = barHeight;
                peakShapes[barIndex].Margin = new Thickness(xCoord, (height - 1) - channelPeakData[barIndex] - peakDotHeight, 0, 0);
                peakShapes[barIndex].Height = peakDotHeight;

                if (channelPeakData[barIndex] > 0.05)
                    allZero = false;

                lastPeakHeight = barHeight;
                barHeight = 0f;
                barIndex++;
            }
        }

        if (allZero && !IsPlaying)
            animationTimer.Stop();
    }

    private void UpdateBarLayout()
    {
        if (soundPlayer == null || spectrumCanvas == null)
            return;

        barWidth = Math.Max(((double)(spectrumCanvas.RenderSize.Width - (BarSpacing * (BarCount + 1))) / (double)BarCount), 1);
        maximumFrequencyIndex = Math.Min(soundPlayer.GetFFTFrequencyIndex(MaximumFrequency) + 1, 2047);
        minimumFrequencyIndex = Math.Min(soundPlayer.GetFFTFrequencyIndex(MinimumFrequency), 2047);
        bandWidth = Math.Max(((double)(maximumFrequencyIndex - minimumFrequencyIndex)) / spectrumCanvas.RenderSize.Width, 1.0);

        int actualBarCount;
        if (barWidth >= 1.0d)
            actualBarCount = BarCount;
        else
            actualBarCount = Math.Max((int)((spectrumCanvas.RenderSize.Width - BarSpacing) / (barWidth + BarSpacing)), 1);
        channelPeakData = new float[actualBarCount];

        int indexCount = maximumFrequencyIndex - minimumFrequencyIndex;
        int linearIndexBucketSize = (int)Math.Round((double)indexCount / (double)actualBarCount, 0);
        List<int> maxIndexList = new List<int>();
        List<int> maxLogScaleIndexList = new List<int>();
        double maxLog = Math.Log(actualBarCount, actualBarCount);
        for (int i = 1; i < actualBarCount; i++)
        {
            maxIndexList.Add(minimumFrequencyIndex + (i * linearIndexBucketSize));
            int logIndex = (int)((maxLog - Math.Log((actualBarCount + 1) - i, (actualBarCount + 1))) * indexCount) + minimumFrequencyIndex;
            maxLogScaleIndexList.Add(logIndex);
        }
        maxIndexList.Add(maximumFrequencyIndex);
        maxLogScaleIndexList.Add(maximumFrequencyIndex);
        barIndexMax = maxIndexList.ToArray();
        barLogScaleIndexMax = maxLogScaleIndexList.ToArray();

        barHeights = new double[actualBarCount];
        peakHeights = new double[actualBarCount];

        spectrumCanvas.Children.Clear();
        barShapes.Clear();
        peakShapes.Clear();

        double height = spectrumCanvas.RenderSize.Height;
        double peakDotHeight = Math.Max(barWidth / 2.0f, 1);
        for (int i = 0; i < actualBarCount; i++)
        {
            double xCoord = BarSpacing + (barWidth * i) + (BarSpacing * i) + 1;
            Rectangle barRectangle = new Rectangle()
            {
                Margin = new Thickness(xCoord, height, 0, 0),
                Width = barWidth,
                Height = 0,
                Style = BarStyle
            };
            barShapes.Add(barRectangle);
            Rectangle peakRectangle = new Rectangle()
            {
                Margin = new Thickness(xCoord, height - peakDotHeight, 0, 0),
                Width = barWidth,
                Height = peakDotHeight,
                Style = PeakStyle
            };
            peakShapes.Add(peakRectangle);
        }

        foreach (Shape shape in barShapes)
            spectrumCanvas.Children.Add(shape);
        foreach (Shape shape in peakShapes)
            spectrumCanvas.Children.Add(shape);

        ActualBarWidth = barWidth;
    }
    #endregion

    #region Event Handlers
    private void animationTimer_Tick(object sender, EventArgs e)
    {
        UpdateSpectrum();
    }
    #endregion
}
