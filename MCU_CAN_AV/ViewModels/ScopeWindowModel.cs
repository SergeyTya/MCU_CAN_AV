using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData.Binding;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using LiveChartsCore.Kernel.Sketches;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using ScottPlot.Colormaps;
using System.Reactive.Disposables;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using System.Reactive.Linq;
using Avalonia.Threading;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class ScopeWindowModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        ObservableCollection<ChannelTemplate>? _channelList;

        [ObservableProperty]
        ObservableCollection<ISeries>? _series;

        [ObservableProperty]
        ObservableCollection<ICartesianAxis>? _yAxes;

        [ObservableProperty]
        ObservableCollection<DateTimeAxis>? _xAxes;

        private static readonly SKColor s_blue = new(25, 118, 210);
        private static readonly SKColor s_red = new(229, 57, 53);
        private static readonly SKColor s_yellow = new(198, 167, 0);


        public ScopeWindowModel()
        {
            Messenger.RegisterAll(this);
            ChannelList = new();

            Series = new() { };
            YAxes = new() { new Axis() { IsVisible =false} };
           


            var _XAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
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

            XAxes = new() { _XAxis };
            init();
        }

        void init() {

            if (IDevice.Current == null) return;
            var tmp = IDevice.Current.DeviceDescription;
            int i = 1;
            foreach (var item in tmp)
            {
                if (!item.IsReadWrite)
                {
                    var tmp_ch = new ChannelTemplate(item, i++);
                    ChannelList?.Add(tmp_ch);
                    YAxes?.Add(tmp_ch.YAxis);
                    Series?.Add(tmp_ch.line);

                }
            }
        }

        public void Receive(ConnectionState message)
        {

            if (message.state == ConnectionState.State.Connected)
            {
                init();
            }

            //var disposable = Observable.Interval(TimeSpan.FromSeconds(0.3)).Subscribe(x =>
            //{
            //    foreach (var l in Series) {
            //        ((LineSeries<DateTimePoint>)l).ScalesXAt = 0;
            //    }
            //});

                //if (message.state == ConnectionState.State.Disconnected)
                //{
                //    if (ChannelList != null)
                //    {

                //        foreach (var item in ChannelList)
                //        {
                //            item.Dispose();
                //        }
                //        ChannelList?.Clear();
                //    }
                //}
            }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (-(date - DateTime.Now)).TotalSeconds;

            return $"{secsAgo:N0}";
        }

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
    }

    public partial class ChannelTemplate : ObservableObject
    {
        IDeviceParameter _item;
        int _axeNumber = 0;

        public Axis YAxis = new Axis() {
            SeparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.Gray,
                StrokeThickness = 0,
                PathEffect = new DashEffect(new float[] { 3, 3 })
            },
            SubseparatorsPaint = new SolidColorPaint
            {
                Color = SKColors.Black,
                StrokeThickness = 0,
            },
            SubseparatorsCount = 9,
            ZeroPaint = new SolidColorPaint
            {
                Color = SKColors.Gray,
                StrokeThickness = 0
            },
            TicksPaint = new SolidColorPaint
            {
                Color = SKColors.Gray,
                StrokeThickness = 0
            },
            SubticksPaint = new SolidColorPaint
            {
                Color = SKColors.Gray,
                StrokeThickness = 0
            }
        };

        List<DateTimePoint> dateTimePoints = new List<DateTimePoint>();
        public LineSeries<DateTimePoint> line = new LineSeries<DateTimePoint>() {
            LineSmoothness = 0,
            Fill = null,
            GeometryStroke = null,
          //  Stroke = new SolidColorPaint(SKColors.Yellow, 2),
          //  GeometryFill = new SolidColorPaint(SKColors.Yellow),
            GeometrySize = 5
        };

        private readonly List<DateTimePoint> _values = new();
        private readonly IDisposable? _disposable;

        public ChannelTemplate(IDeviceParameter item, int axeNumber)
        {
            line.Values = dateTimePoints;
            _axeNumber = axeNumber;
            _item = item;
            ChannelName = item.Name;
            YAxis.Name = ChannelName;
            YAxis.IsVisible = false;
            line.IsVisible = false;
            YAxis.ShowSeparatorLines = true;

            _disposable = item.Value.Subscribe((_) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (IsChannelSelected == false) return;
                    dateTimePoints.Add(new DateTimePoint(DateTime.Now, _));
                    if (dateTimePoints.Count > 250)
                    {
                        dateTimePoints.RemoveAt(0);

                    };

                    line.ScalesYAt = _axeNumber;
                });
            });
        }

        [ObservableProperty]
        bool _isChannelSelected = false;

        [ObservableProperty]
        private string _channelName = "no name";

        [RelayCommand]
        void onChekboxClick() {
            YAxis.IsVisible = IsChannelSelected;
            line.IsVisible = IsChannelSelected;

            if (!IsChannelSelected) {
                dateTimePoints.Clear();
            }
        }

      
    }
}
