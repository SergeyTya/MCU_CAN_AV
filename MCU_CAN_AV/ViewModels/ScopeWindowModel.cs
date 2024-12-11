using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkiaSharp;
using LiveChartsCore.Kernel.Sketches;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using System.Reactive.Linq;
using Avalonia.Threading;


namespace MCU_CAN_AV.ViewModels
{
    internal partial class ScopeWindowModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        private double _periodValue = 10.0;
        [ObservableProperty]
        ObservableCollection<ScopeWindowChannelTemplate> _channelList;

        [ObservableProperty] private ObservableCollection<ISeries>? _series;

        [ObservableProperty]
        private ObservableCollection<ICartesianAxis>? _yAxes;

        [ObservableProperty]
        private ObservableCollection<DateTimeAxis>? _xAxes;

        [ObservableProperty]
        private SolidColorPaint _tooltipTextPaint  = new SolidColorPaint{
         Color = new SKColor(242, 244, 195),
         SKTypeface = SKTypeface.FromFamilyName("Courier New"),
        };

        [ObservableProperty]
        private SolidColorPaint _tooltipBackgroundPaint = new SolidColorPaint(new SKColor(72, 0, 50));

        [ObservableProperty]
        private bool _paused;

        [ObservableProperty]
        private string _period;            

        [RelayCommand]
        private void PauseClick() { 
        }
        
        [RelayCommand]
        private void ScaleChanged() { 
            
            var res = double.TryParse(Period, out var temp);
            if (res) 
            {
                _periodValue = temp;
            }
            else
            {
                Period = _periodValue.ToString("0.0###");
            }
        }

        [ObservableProperty] 
        private Action _scaleEndEditCommand;
        
        [ObservableProperty]
        private bool _isFixed;

        [RelayCommand]
        private void FixedAxis()
        {
            Dispatcher.UIThread.Post(
                     () => {
                         YAxes[0].IsVisible = IsFixed;
                         foreach (var l in ChannelList)
                         {
                            l.IsFixedMode = IsFixed;
                         }
                         ChartPan();
                     });
        }

        [RelayCommand]
        private void ChartPan()
        {
            if (YAxes == null) return;
            foreach (var l in YAxes) {
                if (l.IsVisible) {
                    l.MinLimit = null; l.MaxLimit = null;
                    l.MinLimit = null; l.MaxLimit = null;
                }
            }
        }


        internal static readonly byte[][] ChColors =
        [
            [0xff,0xff,0x11],
            [0x13,0x9f,0xff],
            [0xff,0x69,0x29],
            [0xbf,0x46,0xff]
        ];

        public ScopeWindowModel()
        {
            Period = _periodValue.ToString("0.0###");
            
            Messenger.RegisterAll(this);
            ChannelList = [];

 
            var xAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
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
                    PathEffect = new DashEffect([3, 3])
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

            XAxes = [xAxis];
            YAxes =
            [
                new Axis()
                {
                    IsVisible = false,
                    Name = "Values",
                    SeparatorsPaint = new SolidColorPaint
                    {
                        Color = SKColors.Gray,
                        StrokeThickness = 0,
                        PathEffect = new DashEffect([3, 3])
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
                    ShowSeparatorLines = true,
                }
            ];

            Series =
            [
                new LineSeries<DateTimePoint>()
                {
                    IsVisible = false,
                }
            ];

            Init();
        }

        private void Init() {

            if (IDevice.Current == null) return;
            var tmp = IDevice.Current.DeviceDescription;
            var i = 1;
            foreach (var item in tmp)
            {
                if (item.IsReadWrite) continue;
                var tmpCh = new ScopeWindowChannelTemplate(item, i++);
                ChannelList?.Add(tmpCh);
                YAxes?.Add(tmpCh.YAxis);
                Series?.Add(tmpCh.Line);

                // Set checked state acording channels limit 
                tmpCh.PropertyChanged += (_,a) =>
                {
                    if (a.PropertyName != "IsChannelSelected") return;
                    if (tmpCh.IsChannelSelected) {

                        // Check how many is visible
                        var res = ChannelList?.Where(x => x.IsVisible == true).Select(x => x);
                        if(res == null) return;
                        if (res.Count() < 4)
                        {
                            tmpCh.IsVisible = true;
                        }
                        else {
                            tmpCh.IsChannelSelected = false;
                        }
                    } else {
                        tmpCh.IsVisible = false;
                    }

                    // Apply Yaxis color
                    var res1 = ChannelList?.Where(x => x.IsVisible == true).Select(x => x);
                    if(res1 == null) return;    
                    var k = 0;
                    tmpCh.IsFixedMode = IsFixed;
                    foreach (var el in res1)
                    {
                        if ( IsFixed && k == 0 ) { 

                        } else {
                            el.ScopePosition = k;
                        }
                                
                        k++;
                    }
                    // Disable time axis
                    if (XAxes != null)
                    {
                        XAxes[0].IsVisible = k!= 0; 
                    }
                };

                // Refreshing scope
                var disposable = Observable.Interval(TimeSpan.FromSeconds(0.3)).Subscribe(x =>
                {
                    Dispatcher.UIThread.Post(RefreshScope);
                });

                // Apply new value to channel
                tmpCh.Disposable = item.Value.Subscribe((d) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        tmpCh.AddTimedPoint(d);
                        tmpCh.Period = _periodValue;

                    });
                });
            }
        }

        private void RefreshScope() {
            if(XAxes == null) return;   
            if (!Paused) XAxes[0].CustomSeparators = GetSeparators();
        }

        public void Receive(ConnectionState message)
        {

            if (message.state == ConnectionState.State.Connected)
            {
                Init();
            }

            if (message.state != ConnectionState.State.Disconnected) return;
            
            foreach (var item in ChannelList)
            {
                item.Dispose();
            }
            ChannelList?.Clear();
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

            List<double> retVal = [];
            double size = 5;
            if (_periodValue < 5)
            {
                size = 1;
            }

            if (_periodValue > 100)
            {
                size = 10;
            }

            int max= (int) (_periodValue/size);
            
            for (int j = max+1; j > 0; j--)
            {
                retVal.Add( DateTime.Now.AddSeconds(-j*size).Ticks );
            }
            
            retVal.Add(now.Ticks);

            return retVal.ToArray();
        }
    }

   
}
