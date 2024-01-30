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
using System;
using System.Linq;

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


        public static readonly StyledProperty<double> Cartesian1ValueProperty =
           AvaloniaProperty.Register<MetersControl, double>("Cartesian1Value");
        public double Cartesian1Value
        {
            set => SetValue(Cartesian1ValueProperty, value);
            get => GetValue(Cartesian1ValueProperty);
        }

        public static readonly StyledProperty<double> Cartesian4ValueProperty =
            AvaloniaProperty.Register<MetersControl, double>("Cartesian4Value");
        public double Cartesian4Value
        {
            set => SetValue(Cartesian4ValueProperty, value);
            get => GetValue(Cartesian4ValueProperty);
        }

        public static readonly StyledProperty<string> Angular2LabelProperty =
        AvaloniaProperty.Register<MetersControl, string>("Angular2Label");
        public string Angular2Label
        {
            set => SetValue(Angular2LabelProperty, value);
            get => GetValue(Angular2LabelProperty);
        }

        public static readonly StyledProperty<string> Angular2UnitProperty =
        AvaloniaProperty.Register<MetersControl, string>("Angular2Unit");
        public string Angular2Unit
        {
            set => SetValue(Angular2UnitProperty, value);
            get => GetValue(Angular2UnitProperty);
        }

        public static readonly StyledProperty<double> Angular2ValueProperty =
        AvaloniaProperty.Register<MetersControl, double>("Angular2Value");
        public double Angular2Value
        {
            set => SetValue(Angular2ValueProperty, value);
            get => GetValue(Angular2ValueProperty);
        }

        public static readonly StyledProperty<string> Angular3LabelProperty =
        AvaloniaProperty.Register<MetersControl, string>("Angular3Label");
        public string Angular3Label
        {
            set => SetValue(Angular3LabelProperty, value);
            get => GetValue(Angular3LabelProperty);
        }

        public static readonly StyledProperty<string> Angular3UnitProperty =
        AvaloniaProperty.Register<MetersControl, string>("Angular3Unit");
        public string Angular3Unit
        {
            set => SetValue(Angular3UnitProperty, value);
            get => GetValue(Angular3UnitProperty);
        }

        public static readonly StyledProperty<double> Angular3ValueProperty =
        AvaloniaProperty.Register<MetersControl, double>("Angular3Value");
        public double Angular3Value
        {
            set => SetValue(Angular3ValueProperty, value);
            get => GetValue(Angular3ValueProperty);
        }

        public static readonly StyledProperty<IObservable<int>> Slider1ValueProperty =
           AvaloniaProperty.Register<CustomSlider, IObservable<int>>("Slider1Value");
        public IObservable<int> Slider1Value
        {
            set => SetValue(Slider1ValueProperty, value);
            get => GetValue(Slider1ValueProperty);
        }

        public static readonly StyledProperty<IObservable<int>> Slider2ValueProperty =
            AvaloniaProperty.Register<CustomSlider, IObservable<int>>("Slider2Value");
        public IObservable<int> Slider2Value
        {
            set => SetValue(Slider2ValueProperty, value);
            get => GetValue(Slider2ValueProperty);
        }

        public static readonly StyledProperty<string> Slider1LabelProperty =
           AvaloniaProperty.Register<MetersControl, string>("Slider1Label");
        public string Slider1Label
        {
            set
            {
                Slider1.LabelText = value;
                SetValue(Slider1LabelProperty, value);
            }
            get => GetValue(Slider1LabelProperty);
        }

        public static readonly StyledProperty<double> Slider1MaxlProperty =
            AvaloniaProperty.Register<MetersControl, double>("Slider1Max");
        public double Slider1Max
        {
            set
            {
                Slider1.Max = value;
                SetValue(Slider1MaxlProperty, value);
            }
            get => GetValue(Slider1MaxlProperty);
        }

        public static readonly StyledProperty<double> Slider1MinlProperty =
            AvaloniaProperty.Register<MetersControl, double>("Slider1Min");
        public double Slider1Min
        {
            set
            {
                Slider1.Min = value;
                SetValue(Slider1MinlProperty, value);
            }
            get => GetValue(Slider1MinlProperty);
        }


        public static readonly StyledProperty<string> Slider2LabelProperty =
            AvaloniaProperty.Register<MetersControl, string>("Slider2Label");
        public string Slider2Label
        {
            set
            {
                Slider2.LabelText = value;
                SetValue(Slider2LabelProperty, value);
            }
            get => GetValue(Slider2LabelProperty);
        }

        public static readonly StyledProperty<double> Slider2MaxlProperty =
        AvaloniaProperty.Register<MetersControl, double>("Slider2Max");
        public double Slider2Max
        {
            set
            {
                Slider2.Max = value;
                SetValue(Slider2MaxlProperty, value);
            }
            get => GetValue(Slider2MaxlProperty);
        }

        public static readonly StyledProperty<double> Slider2MinlProperty =
            AvaloniaProperty.Register<MetersControl, double>("Slider2Min");
        public double Slider2Min
        {
            set
            {
                Slider2.Min = value;
                SetValue(Slider2MinlProperty, value);
            }
            get => GetValue(Slider2MinlProperty);
        }



        public static readonly StyledProperty<string> LabelMeter1ValueProperty =
        AvaloniaProperty.Register<MetersControl, string>("LabelMeter1Value");
        public string LabelMeter1Value
        {
            set => SetValue(LabelMeter1ValueProperty, value);
            get => GetValue(LabelMeter1ValueProperty);
        }
        public static readonly StyledProperty<string> LabelMeter2ValueProperty =
        AvaloniaProperty.Register<MetersControl, string>("LabelMeter2Value");
        public string LabelMeter2Value
        {
            set => SetValue(LabelMeter2ValueProperty, value);
            get => GetValue(LabelMeter2ValueProperty);
        }

        public Angular4Indi()
        {
            InitializeComponent();
            Init_angular();
        }

        ColumnSeries<double> cs1 = new()
        {
            Name = "Voltage",
            Values = new double[] { 98 }
        };

        ColumnSeries<double> cs2 = new()
        {
            Name = "Current",
            Values = new double[] { 98 },
            
            
        };

        void Init_angular()
        {

            this.CartesianChart1.Series = new ISeries[] {
            
                new ColumnSeries<double>(){
                    Name = "Voltage",
                    Values = new double[] { 98 }
                }


                
            };

            this.CartesianChart1.YAxes = new Axis[]
            {
                new Axis{
                    MaxLimit = 10,
                    MinLimit = 0,
                }
            };

            this.CartesianChart2.YAxes = new Axis[]
            {
                new Axis{ 
                    MaxLimit = 10,
                    MinLimit = 0,
                }
            };

            this.CartesianChart2.Series = new ISeries[]{

                new ColumnSeries<double>(){
                    Name = "Voltage",
                    Values = new double[] { 98 }
                }
            };


            this.PieChart2.Series = GaugeGenerator.BuildAngularGaugeSections(
                 new GaugeItem(100, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                 new GaugeItem(100, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            var new_value = change.NewValue;



            if (change.Property.Name == "Cartesian1Value")
            {
                CartesianChart1.Series.ToArray()[0].Values = new double[] { Cartesian1Value};
            }

            if (change.Property.Name == "Cartesian4Value")
            {
                CartesianChart2.Series.ToArray()[0].Values = new double[] { Cartesian4Value};  
            }

            if (change.Property.Name == "Angular3Value")
            {
                // ((MetersViewModel)DataContext).Needle_trq.Value = Angular1Value;
                Needle3.Value = Angular3Value;
                this.LabelNamePieChart3.Text = Angular3Label;
                this.LabelValPieChart3.Text = Angular3Value.ToString("#0.0");
                this.LabelUniPieChart3.Text = Angular3Unit;
            }
            if (change.Property.Name == "Angular2Value")
            {
                // ((MetersViewModel)DataContext).Needle_spd.Value = Angular2Value;
                Needle2.Value = Angular3Value;
                this.LabelNamePieChart2.Text = Angular2Label;
                this.LabelValPieChart2.Text  = Angular2Value.ToString("#0.0");
                this.LabelUniPieChart2.Text  = Angular2Unit;
            }

            base.OnPropertyChanged(change);
        }
    }
}
