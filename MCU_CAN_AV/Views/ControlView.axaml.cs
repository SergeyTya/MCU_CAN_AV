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
using MCU_CAN_AV.DeviceDescriprion;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace MCU_CAN_AV.Views
{
    public partial class ControlView : UserControl
    {

        public static readonly StyledProperty<ObservableCollection<IDeviceParameter>> ControlSourceProperty =
          AvaloniaProperty.Register<ControlView, ObservableCollection<IDeviceParameter>>("ControlSource");

        public ObservableCollection<IDeviceParameter> ControlSource
        {
            set
            {
                ControlSource_CollectionChanged(value, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                SetValue(ControlSourceProperty, value);
                if (ControlSource != null) ControlSource.CollectionChanged += ControlSource_CollectionChanged;
            }
            get => GetValue(ControlSourceProperty);
        }

        public void ControlSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            
            var tmp = (ObservableCollection<IDeviceParameter>)sender;
            if (tmp == null) {
                return;
            }

            foreach (var el in tmp)
            {

                if(el.Name == "Actual torque")
                {
                    el.Value.Subscribe((x) => {
                        LabelValPieChart2.Text = x.ToString("0.##");
                        Needle2.Value = x; 
                    });
                    LabelNamePieChart2.Text = "Torque";
                    LabelUniPieChart2.Text = el.Unit;
                }

                if (el.Name == "Actual Speed")
                {
                    el.Value.Subscribe((x) => {
                        LabelValPieChart3.Text = x.ToString("0");
                        Needle3.Value = x;
                    });
                    LabelNamePieChart3.Text = "Speed";
                    LabelUniPieChart3.Text = el.Unit;
                }

                if (el.Name == "Actual Current") {
                    el.Value.Subscribe((x) => {
                        CartesianChart1.Series.ToArray()[0].Values = new double[] { x };
                    });
                    Cartesian1_Label.Text = "Current,"+ el.Unit;
                    //TODO Min MaX!
                }

                if (el.Name == "Actual Voltage")
                {
                    el.Value.Subscribe((x) => {
                        CartesianChart2.Series.ToArray()[0].Values = new double[] { x };
                    });
                    Cartesian2_Label.Text = "Voltage," + el.Unit;
                }

            }

        }

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements2 { get; set; }
        public NeedleVisual Needle2 { get; set; }

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements3 { get; set; }
        public NeedleVisual Needle3 { get; set; }


        public ISeries[] Series1 { get; set; }
        public ISeries[] Series4 { get; set; }


        
        public ControlView()
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


            Needle2 = new NeedleVisual { Value = 3, Fill = new SolidColorPaint(SKColor.Parse("#64576b")) };
            Needle3 = new NeedleVisual { Value = 3, Fill = new SolidColorPaint(SKColor.Parse("#64576b")) };



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
