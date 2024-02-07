using Avalonia;
using Avalonia.Controls;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using MCU_CAN_AV.CustomControls;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Avalonia.Threading;

namespace MCU_CAN_AV.Views
{
    public partial class LoggerWindow : Window
    {

        private readonly List<DateTimePoint> _values = new();
        private readonly DateTimeAxis _customAxis;
        public ObservableCollection<ISeries> Series { get; set; }
        public object Sync { get; } = new object();

        public bool IsReading { get; set; } = true;

        public Axis[] XAxes { get; set; }

     

        public static readonly StyledProperty<string> InputValueProperty =
          AvaloniaProperty.Register<MetersControl, string>("InputValue");
        
        public string InputValue
        {
            set{
                SetValue(InputValueProperty, value); 
            }
            get => GetValue(InputValueProperty);
        }
        public LoggerWindow(string name)
        {
            Name = name;    
            InitializeComponent();
            this.Closed += (_,_) => {
                _Is_Alive = false;
                this.Close();
            };

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<DateTimePoint>
                {
                    Values = _values,
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null
                }
            };

            _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100))
            };

            XAxes = new Axis[] { _customAxis };

            Chart.Series = Series;
            Chart.XAxes = XAxes;
            Chart.SyncContext = Sync;

            Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(x => {
                Dispatcher.UIThread.Invoke((() =>
                {
                    lock (Sync)
                    {
                        _values.Add(new DateTimePoint(DateTime.Now, Double.Parse(InputValue)));
                        if (_values.Count > 250) _values.RemoveAt(0);
                        _customAxis.CustomSeparators = GetSeparators();

                    }
                }));
            });
        }
   
        private static string Formatter(DateTime date)
        {
            var secsAgo = (DateTime.Now - date).TotalSeconds;

            //return secsAgo < 1
            //    ? "now"
            //    : $"{secsAgo:N0}s ago";
            return $"{secsAgo:N0}s";
        }

        private double[] GetSeparators()
        {
            var now = DateTime.Now;

            return new double[]
            {
            now.AddSeconds(-25).Ticks,
            now.AddSeconds(-20).Ticks,
            now.AddSeconds(-15).Ticks,
            now.AddSeconds(-10).Ticks,
            now.AddSeconds(-5).Ticks,
            now.Ticks
            };
        }

        bool _Is_Alive = true;
        public bool Is_Alive { get => _Is_Alive; }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {

            if (change.Property.Name == "InputValue")
            {
                //lock (Sync)
                //{
                //    _values.Add(new DateTimePoint(DateTime.Now, Double.Parse(InputValue)));
                //    if (_values.Count > 250) _values.RemoveAt(0);
                //    _customAxis.CustomSeparators = GetSeparators();

                //}
            }
            base.OnPropertyChanged(change);
        }



    }
}
