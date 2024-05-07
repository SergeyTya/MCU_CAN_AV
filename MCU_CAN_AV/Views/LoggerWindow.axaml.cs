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

        bool _IsAlive = true;
        public bool IsAlive { get => _IsAlive; }

        internal double value_fltr =0;

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

            MainChart.Series = new ISeries[]
            {
                series
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
                    _values.Add(new DateTimePoint(DateTime.Now, tmp));
                    if (_values.Count > 250) _values.RemoveAt(0);

                    _XAxis.CustomSeparators = GetSeparators();

                    this.Indi.Text = InputValue;
                });
            });

            // Form closing event
            this.Closing += (_, __) => { disposable.Dispose(); };


            // Y axis zoom event

            //_YAxis.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            //{
            //    if (e.PropertyName is (nameof(_XAxis.MaxLimit)) or (nameof(_XAxis.MinLimit)))
            //    {
            //        // at this point the axis limits changed 
            //        // the user is using the zooming or panning features 
            //        // or the range was set explicitly in 
            //        if (_maxXVisible != null)
            //        {
            //            if (_maxXVisible == _YAxis.MaxLimit)
            //            {
            //            }
            //            else
            //            {
            //                _minXVisible = _YAxis.MinLimit;
            //                _maxXVisible = _YAxis.MaxLimit;
            //            }
            //        }
            //        else
            //        {
            //        }
            //    }
            //};


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

            // Chart pan buttion
            MenuItemPan.Click += (_, __) => ChatrPan();
        }


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