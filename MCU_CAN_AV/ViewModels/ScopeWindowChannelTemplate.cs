using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using MCU_CAN_AV.Devices;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;

namespace MCU_CAN_AV.ViewModels
{
    public partial class ScopeWindowChannelTemplate : ObservableObject, IDisposable
    {

        public readonly Axis YAxis = new Axis()
        {
            NamePadding = new Padding(0, 0),
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
                ShowSeparatorLines = false,
            //  AnimationsSpeed = TimeSpan.FromMilliseconds(0),
        };


        public readonly LineSeries<DateTimePoint> Line = new LineSeries<DateTimePoint>()
        {
            LineSmoothness = 0,
            Fill = null,
            GeometryStroke = null,
            GeometrySize = 5,
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Coordinate.PrimaryValue:0.####}"
        };

        /***
         * Use it to control subscribe to item.value
         */
        public IDisposable? Disposable;
        private readonly List<DateTimePoint> _dateTimePoints = [];
        private bool _disposed ;
        private bool _isFixedMode ;


        [ObservableProperty]
        private SolidColorBrush? _textColor;

        [ObservableProperty]
        private bool _isChannelSelected ;

        [ObservableProperty]
        private string _channelName = "no name";

        private void SetAxiColor(SKColor paint) {
            YAxis.NamePaint = new SolidColorPaint(paint);
            YAxis.LabelsPaint = new SolidColorPaint(paint);
            YAxis.TicksPaint = new SolidColorPaint(paint);
            YAxis.SubticksPaint = new SolidColorPaint(paint);
            YAxis.ZeroPaint = new SolidColorPaint(paint);
        }


        private readonly int _collectionPosition ;// position in channel list
       
        int _scopePosition = -1; // Position in scope, "-1" - out of scope
        public int ScopePosition 
        {
            set
            {
                _scopePosition = value;
                if (_scopePosition < 0) { return; }
                if (_scopePosition > 3) { return; } //TODO!!
                var paint = new SKColor(ScopeWindowModel.ChColors[value][0], ScopeWindowModel.ChColors[value][1], ScopeWindowModel.ChColors[value][2]);
                Line.Stroke = new SolidColorPaint(paint);
                Line.GeometryFill = new SolidColorPaint(paint);
                SetAxiColor(paint);

                Dispatcher.UIThread.Post(() => TextColor = new SolidColorBrush(new Color(100,
                    ScopeWindowModel.ChColors[value][0],
                    ScopeWindowModel.ChColors[value][1],
                    ScopeWindowModel.ChColors[value][2])));

                if (value > 1)
                {
                    // YAxis.Position = LiveChartsCore.Measure.AxisPosition.End;
                }
            }

            get => _scopePosition;
        }

        bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;

                if (_isFixedMode && _scopePosition != 0) {
                    // Dont visible in fixide mode 
                    YAxis.IsVisible = false;
                    Line.ScalesYAt = 1;
                }
                else {
                    YAxis.IsVisible = _isVisible;
                }
                
                Line.IsVisible = _isVisible;
            }
        }
        public bool IsFixedMode {
            set {
                _isFixedMode = value    ;
                if (IsVisible == false) return;

                if (_isFixedMode)
                {
                    Line.ScalesYAt = 0;
                    YAxis.IsVisible = false;
                }
                else
                {
                    YAxis.IsVisible = true;
                    Line.ScalesYAt = _collectionPosition; // scale 
                }
            } 
        }
        
        private double _periodMs = 5000;
        private double _periodNow = 5000;
        public double Period {
            set
            {
                if( (int) _periodMs == (int) (value * 1000) ) return;
                _dateTimePoints.Clear();
                _periodMs = value * 1000;
                _periodNow = _periodMs;
            }
        }

        
        public void AddTimedPoint(double yvalue) {
            if (IsChannelSelected == false) return;
            var timeNow = DateTime.Now;
            var timeThen = timeNow; 
            if (_dateTimePoints.Count > 0)
            {
                timeThen = _dateTimePoints.Last().DateTime;
            }

            
            
            if (_periodNow > 0 )
            {
                var ts = timeNow - timeThen;
                _periodNow -= ts.TotalMilliseconds;
            }
            else
            {
                _dateTimePoints.RemoveAt(0);
            }
            
            _dateTimePoints.Add(new DateTimePoint(timeNow, yvalue));
        }

        public ScopeWindowChannelTemplate(IDeviceParameter item, int collectionPosition)
        {
            Line.Values = _dateTimePoints;
            _collectionPosition = collectionPosition;
            ChannelName = item.Name;
            YAxis.Name = ChannelName;
            YAxis.IsVisible = false;
            Line.IsVisible = false;
            Line.ScalesYAt = _collectionPosition;
            Line.ScalesXAt = 0;

            Dispatcher.UIThread.Post(() => TextColor = new SolidColorBrush(new Color(100, 0x26, 0x27, 0x38)));
        }


        [RelayCommand]
        private void onChekboxClick()
        {
            if (IsChannelSelected) return;
            _dateTimePoints.Clear();
            Dispatcher.UIThread.Post(() => TextColor = new SolidColorBrush(new Color(100, 0x26, 0x27, 0x38)));
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
                Disposable?.Dispose();
                // Освобождаем управляемые ресурсы
            }
            // освобождаем неуправляемые объекты
            _disposed = true;
        }

        ~ScopeWindowChannelTemplate()
        {
            Dispose(false);
        }
    }
}
