using Avalonia.Controls;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.VisualElements;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using Avalonia;
using System.Collections.Generic;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Kernel;

namespace MCU_CAN_AV.CustomControls
{
    public partial class Angular4Indi : UserControl
    {
        
        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements2 { get; set; }
        public NeedleVisual Needle2 { get; set; }

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements3 { get; set; }
        public NeedleVisual Needle3 { get; set; }


        public ISeries[] Series1 { get; set; }
        public ISeries[] Series4 { get; set; }

        public static readonly StyledProperty<double> Angular1ValueProperty =
           AvaloniaProperty.Register<MetersControl, double>("Angular1Value");
        public double Angular1Value
        {
            set => SetValue(Angular1ValueProperty, value);
            get => GetValue(Angular1ValueProperty);
        }

        public static readonly StyledProperty<double> Angular2ValueProperty =
            AvaloniaProperty.Register<MetersControl, double>("Angular2Value");
        public double Angular2Value
        {
            set => SetValue(Angular2ValueProperty, value);
            get => GetValue(Angular2ValueProperty);
        }

        public static readonly StyledProperty<double> Angular3ValueProperty =
   AvaloniaProperty.Register<MetersControl, double>("Angular3Value");
        public double Angular3Value
        {
            set => SetValue(Angular3ValueProperty, value);
            get => GetValue(Angular3ValueProperty);
        }

        public static readonly StyledProperty<double> Angular4ValueProperty =
            AvaloniaProperty.Register<MetersControl, double>("Angular4Value");
        public double Angular4Value
        {
            set => SetValue(Angular4ValueProperty, value);
            get => GetValue(Angular4ValueProperty);
        }

        public Angular4Indi()
        {
            InitializeComponent();
            Init_angular();
        }

        void Init_angular()
        {

            this.CartesianChart1.Series = new ISeries[] {
                new ColumnSeries<double>
                {
                    Name = "Voltage",
                    Values = new double[] { 98 }
                }
            };

            this.CartesianChart2.Series = new ISeries[] {
                new ColumnSeries<double>
                {
                    Name = "Current",
                    Values = new double[] { 56 }
                }
            };


            this.PieChart2.Series = GaugeGenerator.BuildAngularGaugeSections(
                    new GaugeItem(2, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Blue); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                    new GaugeItem(4, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Yellow); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
            );

            this.PieChart3.Series = GaugeGenerator.BuildAngularGaugeSections(
                    new GaugeItem(2, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Blue); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                    new GaugeItem(4, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Yellow); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
            );

           
            Needle2 = new NeedleVisual { Value = 3, Fill = new SolidColorPaint(SKColors.BlueViolet) };
            Needle3 = new NeedleVisual { Value = 3, Fill = new SolidColorPaint(SKColors.BlueViolet) };



            VisualElements2 = new VisualElement<SkiaSharpDrawingContext>[]
{
                        new AngularTicksVisual{
                            LabelsSize = 16,
                            LabelsOuterOffset = 15,
                            OuterOffset = 65,
                            TicksLength = 20,
                            LabelsPaint = new SolidColorPaint(SKColors.Gray),
                            Stroke =  new SolidColorPaint(SKColors.Gray)
                            
                        },
                        Needle2
            };

            

            VisualElements3 = new VisualElement<SkiaSharpDrawingContext>[]
{
                        new AngularTicksVisual{
                            LabelsSize = 16,
                            LabelsOuterOffset = 15,
                            OuterOffset = 65,
                            TicksLength = 20,
                            LabelsPaint = new SolidColorPaint(SKColors.Gray),
                            Stroke =  new SolidColorPaint(SKColors.Gray)
                        },
                        Needle3
            };


            
            this.PieChart2.VisualElements = VisualElements2;
            this.PieChart3.VisualElements = VisualElements3;
            
          

        }
    }
}
