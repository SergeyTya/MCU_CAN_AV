
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.VisualElements;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using MCU_CAN_AV.Devices;
using Splat;
using MCU_CAN_AV.utils;
using LiveChartsCore.SkiaSharpView;
using System.Collections;
using LiveChartsCore.Measure;
using LiveChartsCore.ConditionalDraw;
using DynamicData.Kernel;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class Control2ViewModel : ObservableRecipient, IRecipient<ConnectionState>, IEnableLogger
    {
        [ObservableProperty]
        double _slider1_value = 0;

        partial void OnSlider1_valueChanged(double value)
        {
            IDevice.Current.InSpeed.writeValue(value);
        }

        [ObservableProperty]
        double _slider2_value = 0;


        partial void OnSlider2_valueChanged(double value)
        {
            IDevice.Current.InTorque.writeValue(value);
        }

        [ObservableProperty]
        public IEnumerable<ISeries> _series1;

        [ObservableProperty]
        public IEnumerable<ISeries> _series2;
        public ISeries[] Series3 { get; set; } = new ISeries[]{

                new ColumnSeries<double>(){
                    Values = new double[] { 98.0 },
                    DataLabelsPaint = new SolidColorPaint(new SKColor(245, 245, 245)),
                    DataLabelsPosition = DataLabelsPosition.Middle,
                    DataLabelsFormatter = point  => $"{point.Coordinate.PrimaryValue}"
                }
            };
        public Axis[] YAxes1 { get; } = new[] { new Axis { TextSize = 0, MinLimit = 0 } };

        public ISeries[] Series4 { get; set; } = new ISeries[]{

                new ColumnSeries<double>(){
                    Values = new double[] { 1.1 },
                    DataLabelsPaint = new SolidColorPaint(new SKColor(245, 245, 245)),
                    DataLabelsPosition = DataLabelsPosition.Middle,
                    DataLabelsFormatter = point  => $"{point.Coordinate.PrimaryValue}"
                }
            };

        public Axis[] YAxes2 { get; } = new[] { new Axis { TextSize = 0, MinLimit = 0 } };

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements1 { get; set; }
        public NeedleVisual Needle1 { get; set; }

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements2 { get; set; }
        public NeedleVisual Needle2 { get; set; }



        public IEnumerable<Axis> Axis2;

        [ObservableProperty]
        IDeviceParameter _dP_Speed;
        [ObservableProperty]
        IDeviceParameter _dP_Volage;
        [ObservableProperty]
        IDeviceParameter _dP_Current;
        [ObservableProperty]
        IDeviceParameter _dP_Torque;

        public Control2ViewModel()
        {


            Messenger.RegisterAll(this);



            Series1 = GaugeGenerator.BuildAngularGaugeSections(
               new GaugeItem(0.25, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
            );

            Series2 = GaugeGenerator.BuildAngularGaugeSections(
                    new GaugeItem(2, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Blue); })
            );


            Needle1 = new NeedleVisual { Value = 3, Fill = new SolidColorPaint(SKColors.LightGray) };
            Needle2 = new NeedleVisual { Value = 3, Fill = new SolidColorPaint(SKColors.LightGray) };

            VisualElements1 = new VisualElement<SkiaSharpDrawingContext>[]
{
                        new AngularTicksVisual{
                            LabelsSize = 20,
                            LabelsOuterOffset = 15,
                            OuterOffset = 70,
                            TicksLength = 20,
                            LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                            Stroke =  new SolidColorPaint(SKColors.LightGray)

                        },
                        Needle2
            };



            VisualElements2 = new VisualElement<SkiaSharpDrawingContext>[]
{
                        new AngularTicksVisual{
                            LabelsSize = 20,
                            LabelsOuterOffset = 15,
                            OuterOffset = 70,
                            TicksLength = 20,
                            LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                            Stroke =  new SolidColorPaint(SKColors.LightGray)
                        },
                        Needle1
            };



        }

        IDisposable disp1;
        IDisposable disp2;
        IDisposable disp3;
        IDisposable disp4;
        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {


                DP_Speed = IDevice.Current.OutSpeed;
                DP_Volage = IDevice.Current.OutVoltage;
                DP_Current = IDevice.Current.OutCurrent;
                DP_Torque = IDevice.Current.OutTorque;

                if (DP_Torque != null)
                {

                    double gauge1_step = (DP_Torque.Max - DP_Torque.Min) / 4;

                    Series1 = GaugeGenerator.BuildAngularGaugeSections(
                       new GaugeItem(gauge1_step, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); }),
                       new GaugeItem(gauge1_step, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                       new GaugeItem(gauge1_step, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                       new GaugeItem(gauge1_step, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
                    );

                }


                Series2 = GaugeGenerator.BuildAngularGaugeSections(
                    new GaugeItem(2, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Blue); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Green); }),
                    new GaugeItem(3, s => { s.OuterRadiusOffset = 120; s.MaxRadialColumnWidth = 10; s.Fill = new SolidColorPaint(SKColors.Red); })
                );

                disp1 = DP_Speed.Value.Subscribe((_) =>
                {
                    Needle1.Value = double.Parse(_.ToString("0.0##"));
                });


                disp2 = DP_Torque.Value.Subscribe((_) =>
                {
                    Needle2.Value = double.Parse(_.ToString("0.0##"));
                });

                disp4 = DP_Current.Value.Subscribe((_) =>
                {

                    if (DP_Current.Max > _) { YAxes1[0].MaxLimit = DP_Current.Max; } else { YAxes1[0].MaxLimit = _; }
                    Series3[0].Values = new double[] { _ };
                });

                disp3 = DP_Volage.Value.Subscribe((_) =>
                {
                    if (DP_Volage.Max > _) { YAxes1[0].MaxLimit = DP_Volage.Max; } else { YAxes1[0].MaxLimit = _; }
                    Series4[0].Values = new double[] { _ };
                });

            }

            if (message.state == ConnectionState.State.Disconnected)
            {
                disp1?.Dispose();
                disp2?.Dispose();
            }
        }
    }
}
