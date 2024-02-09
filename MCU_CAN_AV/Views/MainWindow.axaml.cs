using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Linq;

namespace MCU_CAN_AV.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {


        InitializeComponent();


        MCU_CAN_AV.Devices.IDevice.Init_stage.Subscribe(

            _ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ConnectionView.IsVisible = _;
                });

            });

        
        this.Width = 800;

    }
}
