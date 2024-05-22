using System;
using Avalonia.Controls;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Threading;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using System.Collections.Generic;
using LiveChartsCore.Defaults;
using System.Linq;

namespace MCU_CAN_AV.Views
{
    public partial class LoggerWindow : Window
    {


        public static readonly StyledProperty<string> InputValueProperty =
            AvaloniaProperty.Register<LoggerWindow, string>("InputValue");

        private string? InputValue
        {
            get => GetValue(InputValueProperty);
        }

        private static int s_current;
        private readonly List<DateTimePoint> _values = new();
        private readonly List<DateTimePoint> _values2 = new();

        bool _IsAlive = true;
        public bool IsAlive { get => _IsAlive; }

        internal double value_fltr =0;

        double filtered = 0;
        double time = 0;
        public LoggerWindow(string name)
        {
            InitializeComponent();

            const double timeStep = 0.1;

            // Form closing event
            this.Closed += (_, __) =>
            {

                _IsAlive = false;
                this.Close();
            };

    


            // Char series object

            LineSeries<DateTimePoint> series = new LineSeries<DateTimePoint>
            {
                Values = _values,
                LineSmoothness = 0,
                Fill = null,
                GeometryStroke = null,
                Stroke = new SolidColorPaint(SKColors.Yellow, 2),
                GeometryFill = new SolidColorPaint(SKColors.Yellow),
                GeometrySize = 5
            };

            LineSeries<DateTimePoint> series2 = new LineSeries<DateTimePoint>
            {
                Values = _values2,
                LineSmoothness = 0,
                Fill = null,
                GeometryStroke = null,
                Stroke = new SolidColorPaint(SKColors.Red, 1),
               // GeometryFill = new SolidColorPaint(SKColors.LightBlue),
                GeometrySize = 0,
                IsVisible = false
            };

            MainChart.Series = new ISeries[]
            {
                series , series2
            };


  


                //Title object
                MainChart.Title = new LabelVisual
            {
                Text = name,
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.Azure)
            };

            // X axis object
            _XAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                Name = "Time, s",
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                NamePaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12,
                Padding = new Padding(0),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 0,
                    PathEffect = new DashEffect(new float[] { 3, 3 })
                },
                ZeroPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 2
                },
                TicksPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 1.5f
                },
            };

            // Y axis object
            _YAxis = new Axis
            {
                NamePaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 18,
                Padding = new Padding(5, 0, 15, 0),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                SeparatorsPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 0,
                    PathEffect = new DashEffect(new float[] { 3, 3 })
                },
                SubseparatorsPaint = new SolidColorPaint
                {
                    Color = SKColors.Black,
                    StrokeThickness = 0.5f
                },
                SubseparatorsCount = 9,
                ZeroPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 2
                },
                TicksPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 1.5f
                },
                SubticksPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 0
                }
            };

            // Create Axis
            MainChart.XAxes = new Axis[] { _XAxis };
            MainChart.YAxes = new Axis[] { _YAxis };


            // Chart update event
            var disposable = Observable.Interval(TimeSpan.FromSeconds(timeStep)).Subscribe(x =>
            {

                Dispatcher.UIThread.Post(() =>
                {
                    if (InputValue == null) return;
                    var tmp = Double.Parse(InputValue);
  

                    if (time == 0) {
                        filtered = tmp;
                    } else {
                        filtered += (tmp - filtered) * timeStep * 1.5;
                    }
                   
                    _values.Add(new DateTimePoint(DateTime.Now, tmp));
                    _values2.Add(new DateTimePoint(DateTime.Now, filtered));

                    if (_values.Count > 250)
                    {
                        _values.RemoveAt(0);
                        _values2.RemoveAt(0);
                    }

                    this.Indi.Text = $"Avr {filtered.ToString("#.00")} ";

                   if( ! (bool )Btn_pause.IsChecked)  _XAxis.CustomSeparators = GetSeparators();

                    time += timeStep;

                });
            });

            // Form closing event
            this.Closing += (_, __) => { disposable.Dispose(); };


            // Y axis zoom event

    

            MainChart.PointerWheelChanged += (s, e) =>
            {

                if (_YAxis.MinLimit == null || _YAxis.MaxLimit == null)
                {
                    MainChart.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.Y;

                    _zoomStep = _YAxis.MaxLimit / 10;
                    _zoomDelta = (_YAxis.MaxLimit - _YAxis.MinLimit)*0/5;
                    return;
                }

                MainChart.ZoomMode = LiveChartsCore.Measure.ZoomAndPanMode.None;

                bool lockHi = false;
                bool lockLo = false;




                if (e.Delta.Y > 0)
                {
                    if (CheckBoxLockHi.IsChecked == false)
                    {
                        var new_val = _YAxis.MaxLimit + 1;
                        if(new_val > (_YAxis.MinLimit) ) _YAxis.MaxLimit = new_val;

                    }
                    else {
                       // _YAxis.MaxLimit = _YAxis.MaxLimit + _zoomStep / 2;
                    }

                    if (CheckBoxLockLo.IsChecked == false) 
                    {
                        var new_val = _YAxis.MinLimit - 1;
                        if (new_val < (_YAxis.MaxLimit)) 
                            _YAxis.MinLimit  = new_val;
                    }
                    else{
                      //  _YAxis.MinLimit = _YAxis.MinLimit + _zoomStep / 2;
                    }

                }
                else

                if (e.Delta.Y < 0)
                {
                    if (CheckBoxLockHi.IsChecked == false)
                    {
                        var new_val = _YAxis.MaxLimit - 1;
                        if (new_val > (_YAxis.MinLimit))
                            _YAxis.MaxLimit = new_val;
                    }
                    else
                    {
                        //_YAxis.MaxLimit = _YAxis.MaxLimit + _zoomStep / 2;
                    }

                    if (CheckBoxLockLo.IsChecked == false) 
                    {
                        var new_val = _YAxis.MinLimit + 1;
                        if (new_val < (_YAxis.MaxLimit)) 
                            _YAxis.MinLimit = new_val;
                    }
                    else
                    {
                       // _YAxis.MinLimit = _YAxis.MinLimit + _zoomStep / 2;
                    }
                }
            };

            // Chart pan button
            Btn_pan.Click += (_, __) => ChatrPan();

            Btn_pause.Click += (_, __) =>
            {
                bool state = (bool)Btn_pause.IsChecked;
                pause = state;
            };

            Btn_fltr.Click += (_, __) =>
            {
                series2.IsVisible = (bool)Btn_fltr.IsChecked;
            };
        }

        private bool pause = false;
        private double? _zoomDelta = null;
        private double? _zoomStep = null;
        private double? _minXVisible = null;
        private double? _maxXVisible = null;

        public void ChatrPan()
        {
            _YAxis.MinLimit = null; _YAxis.MaxLimit = null;
            _XAxis.MinLimit = null; _XAxis.MaxLimit = null;
        }

        private Random r = new Random();
        private readonly DateTimeAxis _XAxis;
        private readonly Axis _YAxis;

        private double[] GetSeparators()
        {
            var now = DateTime.Now;

            double[] tmp = new double[20];

            for (int i = 0; i < 20; i++)
            {
                tmp[i] = now.AddSeconds(-i * 5).Ticks;
            }

            return tmp;
        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (-(date - DateTime.Now)).TotalSeconds;

            return $"{secsAgo:N0}";
        }
    }
}