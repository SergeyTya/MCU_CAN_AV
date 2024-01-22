using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Extensions;
using SkiaSharp;
using Avalonia;
using System.Reflection.Emit;
using MCU_CAN_AV.ViewModels;
using static MCU_CAN_AV.ViewModels.MetersViewModel;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System;
using MCU_CAN_AV.Models;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections.Generic;
using ReactiveUI;

namespace MCU_CAN_AV.CustomControls
{
    public partial class MetersControl : UserControl
    {
   
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


        public static readonly StyledProperty<ObservableCollection<string>> FaultTableProperty =
            AvaloniaProperty.Register<MetersControl, ObservableCollection<string>>("FaultTable");
        public ObservableCollection<string> FaultTable
        {
            set => SetValue(FaultTableProperty, value);
            get => GetValue(FaultTableProperty);
        }

        //ItemsSource="{Binding $parent[1].Faults}"
        //   Binding="{Binding Name}"

        public MetersControl()
        {
            InitializeComponent();
           
           

          //  Faults = new ObservableCollection<Fault>(new List<Fault>());

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
            if (change.Property.Name == "FaultTable")
            {
                // Bind data to dataGrid from property
                if (DataGrid_Faults.ItemsSource != FaultTable)
                {
                    DataGrid_Faults.ItemsSource = FaultTable;
                }
            }
            base.OnPropertyChanged(change);
        }
    }
}
