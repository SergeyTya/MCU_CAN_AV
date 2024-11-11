﻿using CommunityToolkit.Mvvm.ComponentModel;
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
using LiveChartsCore.Geo;
using System.Reactive.Subjects;
using ReactiveUI;
using Avalonia.Media;
using Newtonsoft.Json.Linq;
using ScottPlot;
using Avalonia.Controls.Shapes;
using LiveChartsCore.Kernel;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class ScopeWindowModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        ObservableCollection<ChannelTemplate> _channelList;

        [ObservableProperty]
        ObservableCollection<ISeries>? _series;

        [ObservableProperty]
        ObservableCollection<ICartesianAxis>? _yAxes;

        [ObservableProperty]
        ObservableCollection<DateTimeAxis>? _xAxes;

        [ObservableProperty]
        public SolidColorPaint _tooltipTextPaint  = new SolidColorPaint{
         Color = new SKColor(242, 244, 195),
         SKTypeface = SKTypeface.FromFamilyName("Courier New"),
        };

        [ObservableProperty]
        public SolidColorPaint _tooltipBackgroundPaint = new SolidColorPaint(new SKColor(72, 0, 50));

        [ObservableProperty]
        public bool _paused;


        [RelayCommand]
        void PauseClick() { 
        
        }

        internal static readonly byte[][] chColors = {
           new byte[] { 0xff,0xff,0x11 },
           new byte[]  { 0x13,0x9f,0xff },
           new byte[]   { 0xff,0x69,0x29 },
           new byte[]    { 0xbf,0x46,0xff },
        };

        public ScopeWindowModel()
        {
            Messenger.RegisterAll(this);
            ChannelList = new();

            

            var _XAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                Name = "Time, s",
                NameTextSize = 16,
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                NamePaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 14,
                Padding = new Padding(0),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                ShowSeparatorLines = true,
                IsVisible = false,
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
            YAxes = new() { new Axis() { IsVisible = false } };

            Series = new() { new LineSeries<DateTimePoint>() {
                IsVisible = false,    
            }};

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


                    tmp_ch.PropertyChanged += (_,__) => {
                        if (__.PropertyName == "IsChannelSelected") {

                            if (tmp_ch.IsChannelSelected) {

                                var res = ChannelList.Where(x => x.IsVisible == true).Select(x => x);
                                if (res.Count() < 4)
                                {
                                    tmp_ch.IsVisible = true;
                                }
                                else {
                                    tmp_ch.IsChannelSelected = false;
                                }
                            } else {
                                tmp_ch.IsVisible = false;
                            }

                            var res1 = ChannelList.Where(x => x.IsVisible == true).Select(x => x);
                            int i = 0;

                            foreach (var item in res1)
                            {
                                item.position = i++;
                            }

                            XAxes[0].IsVisible = i!= 0;

                        } 
                    };

                    tmp_ch._disposable = item.Value.Subscribe((_) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (tmp_ch.IsChannelSelected == false) return;
                            tmp_ch.dateTimePoints.Add(new DateTimePoint(DateTime.Now, _));
                            if (tmp_ch.dateTimePoints.Count > 250)
                            {
                                tmp_ch.dateTimePoints.RemoveAt(0);
                            };
                            if (!Paused) XAxes[0].CustomSeparators = GetSeparators();
                        });
                    });
                }
            }
        }

        public void Receive(ConnectionState message)
        {

            if (message.state == ConnectionState.State.Connected)
            {
                init();
            }

            var disposable = Observable.Interval(TimeSpan.FromSeconds(0.3)).Subscribe(x =>
            {
                foreach (var l in Series)
                {
                    ((LineSeries<DateTimePoint>)l).ScalesXAt = 0;
                }
            });

            if (message.state == ConnectionState.State.Disconnected)
            {
                if (ChannelList != null)
                {

                    foreach (var item in ChannelList)
                    {
                        item.Dispose();
                    }
                    ChannelList?.Clear();
                }
            }
        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (DateTime.Now - date).TotalSeconds;

            return secsAgo < 1
                ? "0"
                : $"{secsAgo:N0}";
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
    }

    public partial class ChannelTemplate : ObservableObject, IDisposable
    {

        public Axis YAxis = new Axis() {
            NamePadding = new(0,0),
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
            },
            TextSize = 12,
            Padding = new Padding(0, 0, 10, 0),
            LabelsPaint = new SolidColorPaint(SKColors.White),
            ShowSeparatorLines = false,
          //  AnimationsSpeed = TimeSpan.FromMilliseconds(0),
        };


        public LineSeries<DateTimePoint> line = new LineSeries<DateTimePoint>() {
            LineSmoothness = 0,
            Fill = null,
            GeometryStroke = null,
            GeometrySize = 5,
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            XToolTipLabelFormatter = (chartPoint) => $"",
            YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Coordinate.PrimaryValue:0.####}"
        };


        IDeviceParameter _item;
        internal int _axeNumber = 0;
        internal List<DateTimePoint> dateTimePoints = new List<DateTimePoint>();
        private readonly List<DateTimePoint> _values = new();
        internal IDisposable? _disposable;
        private bool _disposed = false;

        bool _isVisible;
        public bool IsVisible{ 
            get { 
                return _isVisible; 
            }
            set {
                _isVisible = value;
                YAxis.IsVisible = _isVisible;
                line.IsVisible = _isVisible;
            }
        }

        [ObservableProperty]
        private SolidColorBrush? _textColor;

        public int position {
            set {

                var paint = new SKColor(ScopeWindowModel.chColors[value][0], ScopeWindowModel.chColors[value][1], ScopeWindowModel.chColors[value][2]);
                line.Stroke = new SolidColorPaint(paint);
                line.GeometryFill = new SolidColorPaint(paint);
                YAxis.NamePaint = new SolidColorPaint(paint);
                YAxis.LabelsPaint  = new SolidColorPaint(paint);
                YAxis.TicksPaint = new SolidColorPaint(paint);
                YAxis.SubticksPaint = new SolidColorPaint(paint);
                YAxis.ZeroPaint = new SolidColorPaint(paint);

                Dispatcher.UIThread.Post(() => TextColor = new(new(100,
                    ScopeWindowModel.chColors[value][0], 
                    ScopeWindowModel.chColors[value][1], 
                    ScopeWindowModel.chColors[value][2]),
                1));

                if (value > 1) {
                   // YAxis.Position = LiveChartsCore.Measure.AxisPosition.End;
                }
            }
        }

        public ChannelTemplate(IDeviceParameter item, int axeNumber)
        {
            line.Values = dateTimePoints;
            _axeNumber = axeNumber;
            _item = item;
            ChannelName = item.Name;
            YAxis.Name = ChannelName;
            YAxis.IsVisible = false;
            line.IsVisible = false;
            line.ScalesYAt = _axeNumber;
            line.ScalesXAt = 0;

            Dispatcher.UIThread.Post(() => TextColor = new(new(100, 0x26, 0x27, 0x38), 1));
        }

        [ObservableProperty]
        bool _isChannelSelected = false;

        [ObservableProperty]
        private string _channelName = "no name";

        [RelayCommand]
        void onChekboxClick() {

            if (!IsChannelSelected)
            {
                dateTimePoints.Clear();
                Dispatcher.UIThread.Post(() => TextColor = new(new(100, 0x26, 0x27, 0x38), 1));
            }
        }

        public void Dispose()
        {

            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _disposable?.Dispose();
                // Освобождаем управляемые ресурсы
            }
            // освобождаем неуправляемые объекты
            _disposed = true;
        }

        ~ChannelTemplate()
        {
            Dispose(false);
        }


    }
}
