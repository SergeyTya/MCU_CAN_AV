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


        IDeviceParameter _item;

        internal int _axeNumber = 0;
        internal List<DateTimePoint> dateTimePoints = new List<DateTimePoint>();
        private readonly List<DateTimePoint> _values = new();
        internal IDisposable? _disposable;
        private bool _disposed = false;


        [ObservableProperty]
        private SolidColorBrush? _textColor;

        public int position
        {
            set
            {

                var paint = new SKColor(ScopeWindowModel.chColors[value][0], ScopeWindowModel.chColors[value][1], ScopeWindowModel.chColors[value][2]);
                line.Stroke = new SolidColorPaint(paint);
                line.GeometryFill = new SolidColorPaint(paint);
                YAxis.NamePaint = new SolidColorPaint(paint);
                YAxis.LabelsPaint = new SolidColorPaint(paint);
                YAxis.TicksPaint = new SolidColorPaint(paint);
                YAxis.SubticksPaint = new SolidColorPaint(paint);
                YAxis.ZeroPaint = new SolidColorPaint(paint);

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
                YAxis.IsVisible = _isVisible;
                line.IsVisible = _isVisible;
            }
        }

        [ObservableProperty]
        bool _isChannelSelected = false;

        [ObservableProperty]
        private string _channelName = "no name";

        internal void fix(bool state) // fixing line to axis #1
        {
            if (IsVisible == false) return;

            if (state)
            {
                line.ScalesYAt = 1;
                if (_axeNumber != 1)
                {
                    YAxis.IsVisible = false;
                }
                else
                {
                    YAxis.Name = "";

                }
            }
            else
            {
                line.ScalesYAt = _axeNumber;
                YAxis.IsVisible = true;
                YAxis.Name = ChannelName;
            }

        }

        public ScopeWindowChannelTemplate(IDeviceParameter item, int axeNumber)
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


        [RelayCommand]
        void onChekboxClick()
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
                _disposable?.Dispose();
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
