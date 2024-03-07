using Avalonia.Controls;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.VisualElements;


namespace MCU_CAN_AV.Views
{
    public partial class LoggerWindow : Window
    {

        private readonly ObservableCollection<int> _values;
        public LoggerWindow(string Name)
        {
            InitializeComponent();

            _values = new ObservableCollection<int>();

            chart.Series = new ISeries[]
            {
            new LineSeries<int>
            {
                Values = _values,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
            }
            };


            chart.Title = new LabelVisual
            {
                Text = Name,
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };
        }

    }



    }
}
