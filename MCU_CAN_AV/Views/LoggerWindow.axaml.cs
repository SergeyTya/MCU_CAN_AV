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

        public LoggerWindow(string name)
        {
            InitializeComponent();

            const double timeStep = 0.1;

           



            MainChart.Series = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Values = _values,
                    LineSmoothness = 0,
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    Stroke =  new SolidColorPaint(SKColors.Yellow, 2)
                }
            };

            MainChart.Title = new LabelVisual
            {
                Text = name,
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.Azure)
            };

            _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                Name = "Time, s",
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                NamePaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12,
                Padding = new Padding(5, 5, 5, 5),
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

            MainChart.XAxes = new Axis[] { _customAxis };

            MainChart.YAxes = new[]
            {
                new Axis
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
                }
            };

            var disposable = Observable.Interval(TimeSpan.FromSeconds(timeStep)).Subscribe(x =>
            {

                Dispatcher.UIThread.Post(() =>
                {
                    if (InputValue == null) return;

                    var tmp = Double.Parse(InputValue);
                    _values.Add(new DateTimePoint(DateTime.Now, tmp));
                     if(_values.Count>250) _values.RemoveAt(0);

                    _customAxis.CustomSeparators = GetSeparators();
                });
            });

            this.Closing += (_, __) => { disposable.Dispose(); };
        }

        private Random r = new Random();
        private readonly DateTimeAxis _customAxis;

        private double[] GetSeparators()
        {
            var now = DateTime.Now;

            double[] tmp = new double[20];

            for (int i = 0; i < 20; i++)
            {
                tmp[i] = now.AddSeconds(-i*5).Ticks;
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