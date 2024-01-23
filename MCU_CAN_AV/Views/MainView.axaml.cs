using Avalonia;
using Avalonia.Controls;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.ViewModels;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace MCU_CAN_AV.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        var LabelMeter1Value = Meters.Slider1Value.Subscribe((_) =>
        {
            ((MainViewModel)DataContext).slider_speed = _;
        });

        var LabelMeter2Value = Meters.Slider2Value.Subscribe((_) =>
        {
            ((MainViewModel)DataContext).slider_torque = _;
        });
    }
}
