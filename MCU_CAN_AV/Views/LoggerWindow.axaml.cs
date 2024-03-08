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
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting.Effects;

using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;


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

        private readonly ObservableCollection<int> _values = new();

        public LoggerWindow(string name)
        {
            InitializeComponent();

            const double timeStep = 0.1;
            
            MainChart.Series = new ISeries[]
            {
                new LineSeries<int>
                { 
                    Values = _values,
                    LineSmoothness = 1,
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
            
           // MainChart.DrawMargin = new(70, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto, LiveChartsCore.Measure.Margin.Auto);

            MainChart.XAxes = new[]
            {
                new Axis
                {
                   // MaxLimit = 10,
                   // MinLimit = 0,
                   
                    Name = "Time, s",
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

            MainChart.YAxes = new[]
            {
                new Axis
                {
                   // MaxLimit = 10,
                   // MinLimit = 0,
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
                        if (_values.Count > 50) _values.RemoveAt(0);
                        //var tmp = Double.Parse(InputValue);
                        int tmp = r.Next(-100, 100);
                        _values.Add(tmp);
                        
                    });
                    
            });
            
            
            
            this.Closing += (_, __) => { disposable.Dispose(); };
        }

        private Random r = new Random();
    }
}