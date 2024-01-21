using Avalonia.Controls;
using MCU_CAN_AV.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Extensions;
using SkiaSharp;


namespace MCU_CAN_AV.Views
{
    public partial class MeterWindow : Window
    {
        public MeterWindow()
        {
            //InitializeComponent();
            //var model = new MCU_CAN_AV.ViewModels.MetersViewModel();
            //DataContext = model;

            //Init_angular();

            //model.Faults.Add(new ViewModels.MetersViewModel.Fault("Fault1"));
            //model.Faults.Add(new ViewModels.MetersViewModel.Fault("Fault2"));

            //Label_Torque.Text = string.Format("Torque {0} %", 10.2);
            //Label_Speed.Text = string.Format("Speed {0} krpm", 1.2);

            //Label_Current.Text = string.Format("Current {0} A", 10.2);
            //Label_Voltage.Text = string.Format("Voltage {0} %", 1.2);
 
        }

        private void Init_angular() {
           
           
            //var viewModel_trq1 = new ViewModelAngular();
            //var viewModel_spd1 = new ViewModelAngular();


            //this.PieChartTorque.Series = GaugeGenerator.BuildAngularGaugeSections(
            //        new GaugeItem(100, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
            //        new GaugeItem(100, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
            //    );
            //this.PieChartTorque.VisualElements = viewModel_trq1.VisualElements;

            //this.PieChartSpeed.Series = GaugeGenerator.BuildAngularGaugeSections(
            //        new GaugeItem(2, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Blue); }),
            //        new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
            //        new GaugeItem(4, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Yellow); }),
            //        new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
            //    );
            //this.PieChartSpeed.VisualElements = viewModel_spd1.VisualElements;

        }
    }
}
