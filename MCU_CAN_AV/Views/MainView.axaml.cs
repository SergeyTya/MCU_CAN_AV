using Avalonia;
using Avalonia.Controls;
using MCU_CAN_AV.Can;
using System;
using System.Diagnostics;

namespace MCU_CAN_AV.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        try {

            var CAN = ICAN.Create(
                    new ICAN.CANInitStruct(
                 DevId: 0, CANId: 0, Baudrate: 500, RcvCode: 0, Mask: 0xffffffff, Interval: 100
            ));


            IDisposable listener = CAN.Start().Subscribe(
            (_) => {
                Debug.WriteLine(_.id);
            });


        } catch (ICAN.ICANException e) {
            Debug.WriteLine(e);
            //System.Environment.Exit(1);
        }



    }
}
