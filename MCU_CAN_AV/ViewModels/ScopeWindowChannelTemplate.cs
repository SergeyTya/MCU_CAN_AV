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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using static MCU_CAN_AV.ViewModels.ConnectionState;

namespace MCU_CAN_AV.ViewModels
{
    public partial class ScopeWindowChannelTemplate : ObservableObject, IDisposable
    {

        public Axis YAxis = new Axis()
        {
            NamePadding = new(0, 0),
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


        public LineSeries<DateTimePoint> line = new LineSeries<DateTimePoint>()
        {
            LineSmoothness = 0,
            Fill = null,
            GeometryStroke = null,
            GeometrySize = 5,
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            XToolTipLabelFormatter = (chartPoint) => $"",
            YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Coordinate.PrimaryValue:0.####}"
        };

        /***
         * Use it to control subscribe to item.value
         */
        public IDisposable? disposable;

        private IDeviceParameter _item;
        
       
        private List<DateTimePoint> dateTimePoints = new List<DateTimePoint>();
        private readonly List<DateTimePoint> _values = new();    
        private bool _disposed = false;
        private bool _isFixedMode = false;


        [ObservableProperty]
        private SolidColorBrush? _textColor;

        [ObservableProperty]
        private bool _isChannelSelected = false;

        [ObservableProperty]
        private string _channelName = "no name";

        private void setAxiColor(SKColor paint) {
            YAxis.NamePaint = new SolidColorPaint(paint);
            YAxis.LabelsPaint = new SolidColorPaint(paint);
            YAxis.TicksPaint = new SolidColorPaint(paint);
            YAxis.SubticksPaint = new SolidColorPaint(paint);
            YAxis.ZeroPaint = new SolidColorPaint(paint);
        }


        private int _collectionPosition = 0;// position in channel list
       
        int _scopePosition = -1; // Position in scope, "-1" - out of scope
        public int scopePosition 
        {
            set
            {
                _scopePosition = value;
                if (_scopePosition < 0) { return; }
                if (_scopePosition > 3) { return; } //TODO!!
                var paint = new SKColor(ScopeWindowModel.chColors[value][0], ScopeWindowModel.chColors[value][1], ScopeWindowModel.chColors[value][2]);
                line.Stroke = new SolidColorPaint(paint);
                line.GeometryFill = new SolidColorPaint(paint);
                setAxiColor(paint);

                Dispatcher.UIThread.Post(() => TextColor = new(new(100,
                    ScopeWindowModel.chColors[value][0],
                    ScopeWindowModel.chColors[value][1],
                    ScopeWindowModel.chColors[value][2]),
                1));

                if (value > 1)
                {
                    // YAxis.Position = LiveChartsCore.Measure.AxisPosition.End;
                }
            }

            get { return _scopePosition; }
        }

        bool _isVisible;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;

                if (_isFixedMode && _scopePosition != 0) {
                    // Dont visible in fixide mode 
                    YAxis.IsVisible = false;
                    line.ScalesYAt = 1;
                }
                else {
                    YAxis.IsVisible = _isVisible;
                }
                
                line.IsVisible = _isVisible;
            }
        }
        public bool isFixedMode {
            set {
                _isFixedMode = value    ;
                if (IsVisible == false) return;

                if (_isFixedMode)
                {
                    line.ScalesYAt = 0;
                    YAxis.IsVisible = false;
                }
                else
                {
                    YAxis.IsVisible = true;
                    line.ScalesYAt = _collectionPosition; // scale 
                }
            } 
        }

        public void addTimedPoint(double Yvalue) {
            if (IsChannelSelected == false) return;
            dateTimePoints.Add(new DateTimePoint(DateTime.Now, Yvalue));
            if (dateTimePoints.Count > 250)
            {
                dateTimePoints.RemoveAt(0);
            }
        }

        public ScopeWindowChannelTemplate(IDeviceParameter item, int collectionPosition)
        {
            line.Values = dateTimePoints;
            _collectionPosition = collectionPosition;
            _item = item;
            ChannelName = item.Name;
            YAxis.Name = ChannelName;
            YAxis.IsVisible = false;
            line.IsVisible = false;
            line.ScalesYAt = _collectionPosition;
            line.ScalesXAt = 0;

            Dispatcher.UIThread.Post(() => TextColor = new(new(100, 0x26, 0x27, 0x38), 1));
        }


        [RelayCommand]
        private void onChekboxClick()
        {

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
                disposable?.Dispose();
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
