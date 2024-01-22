using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Extensions;
using SkiaSharp;
using Avalonia;
using System.Reflection.Emit;
using MCU_CAN_AV.ViewModels;

namespace MCU_CAN_AV.CustomControls
{
    public partial class MetersControl : UserControl
    {

        public static readonly StyledProperty<double> Angular1ValueProperty =
            AvaloniaProperty.Register<MetersControl, double>("Angular1Value");
        public static readonly StyledProperty<double> Angular2ValueProperty =
            AvaloniaProperty.Register<MetersControl, double>("Angular2Value");
        public static readonly StyledProperty<string> LabelMeter1ValueProperty =
            AvaloniaProperty.Register<MetersControl, string>("LabelMeter1Value");
        public static readonly StyledProperty<string> LabelMeter2ValueProperty =
       AvaloniaProperty.Register<MetersControl, string>("LabelMeter2Value");

        public double Angular1Value
        {
            set => SetValue(Angular1ValueProperty, value);
            get => GetValue(Angular1ValueProperty);
        }
        public double Angular2Value
        {
            set => SetValue(Angular2ValueProperty, value);
            get => GetValue(Angular2ValueProperty);
        }
        public string LabelMeter1Value
        {
            set => SetValue(LabelMeter1ValueProperty, value);
            get => GetValue(LabelMeter1ValueProperty);
        }
        public string LabelMeter2Value
        {
            set => SetValue(LabelMeter2ValueProperty, value);
            get => GetValue(LabelMeter2ValueProperty);
        }

        public MetersControl()
        {
            InitializeComponent();

            this.PieChartTorque.Series = GaugeGenerator.BuildAngularGaugeSections(
                    new GaugeItem(100, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                    new GaugeItem(100, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
                );

            this.PieChartSpeed.Series = GaugeGenerator.BuildAngularGaugeSections(
                    new GaugeItem(2, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Blue); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                    new GaugeItem(4, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Yellow); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
                );
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            var new_value = change.NewValue;

            if (change.Property.Name == "Angular1Value")
            {
                ((MetersViewModel)DataContext).Needle_trq.Value = Angular1Value;
                this.Label_Torque.Text = string.Format("Torque {0} %", Angular1Value);
            }
            if (change.Property.Name == "Angular2Value")
            {
                ((MetersViewModel)DataContext).Needle_spd.Value = Angular2Value;
                this.Label_Speed.Text = string.Format("Speed {0} krpm", Angular2Value);
            }
            if (change.Property.Name == "LabelMeter1Value")
            {
                this.LabelMeter1.Text = LabelMeter1Value;
            }
            if (change.Property.Name == "LabelMeter2Value")
            {
                this.LabelMeter2.Text = LabelMeter2Value;
            }


            base.OnPropertyChanged(change);
        }
    }
}
