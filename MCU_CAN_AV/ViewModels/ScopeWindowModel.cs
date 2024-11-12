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
using LiveChartsCore.Geo;
using System.Reactive.Subjects;
using ReactiveUI;
using Avalonia.Media;
using Newtonsoft.Json.Linq;
using ScottPlot;
using Avalonia.Controls.Shapes;
using LiveChartsCore.Kernel;
using System.Runtime.CompilerServices;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class ScopeWindowModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        ObservableCollection<ScopeWindowChannelTemplate> _channelList;

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

        [ObservableProperty]
        public bool _isFixed;

        [RelayCommand]
        void fixedAxis()
        {
            Dispatcher.UIThread.Post(
                     () => {
                         foreach (var l in ChannelList)
                         {
                            l.fix(IsFixed);
                         }
                     });
        }

        [RelayCommand]
        public void ChartPan()
        {
            YAxes[1].MinLimit = null; YAxes[1].MaxLimit = null;
            YAxes[1].MinLimit = null; YAxes[1].MaxLimit = null;
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
                    var tmp_ch = new ScopeWindowChannelTemplate(item, i++);
                    ChannelList?.Add(tmp_ch);
                    YAxes?.Add(tmp_ch.YAxis);
                    Series?.Add(tmp_ch.line);

                    // Set checked state acording channels limit 
                    tmp_ch.PropertyChanged += (_,__) => {
                        if (__.PropertyName == "IsChannelSelected") {

                            if (tmp_ch.IsChannelSelected) {

                                // Check how many is visible
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

                            // Apply Yaxis color
                            var res1 = ChannelList.Where(x => x.IsVisible == true).Select(x => x);
                            int i = 0;
                            foreach (var el in res1)
                            {
                                if ( !(IsFixed && i == 0) ) el.color = i;
                                i++;
                            }
                            // Disable time axis
                            XAxes[0].IsVisible = i!= 0;
                        } 
                    };

                    // Refreshing scope
                    var disposable = Observable.Interval(TimeSpan.FromSeconds(0.125)).Subscribe(x =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            RefreshScope();
                        });
                    });

                    // Apply new value to channel
                    tmp_ch.disposable = item.Value.Subscribe((_) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            tmp_ch.addTimedPoint(_);

                        });
                    });
                }
            }
        }

        public void RefreshScope() {
            if (!Paused) XAxes[0].CustomSeparators = GetSeparators();
        }

        public void Receive(ConnectionState message)
        {

            if (message.state == ConnectionState.State.Connected)
            {
                init();
            }

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

   
}
