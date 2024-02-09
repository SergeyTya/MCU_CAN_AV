using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using MCU_CAN_AV.CustomControls;
using MCU_CAN_AV.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCU_CAN_AV.Views
{
    public partial class ConnectionView : UserControl
    {
        public static readonly StyledProperty<bool> ControlsEnableProperty =
         AvaloniaProperty.Register<ConnectionView, bool>("ControlsEnable");
        public bool ControlsEnable
        {
            set
            {
                SetValue(ControlsEnableProperty, value);
                Panel_setup.IsEnabled = value;
                Btn_connect.IsVisible = value;
            }
            get => GetValue(ControlsEnableProperty);
        }

        public static readonly StyledProperty<int> SelectedTabProperty =
        AvaloniaProperty.Register<ConnectionView, int>("SelectedTab");
        public int SelectedTab
        {
            set
            {
                SetValue(SelectedTabProperty, value);
                TabControl1.SelectedIndex = value;
            }
            get => GetValue(SelectedTabProperty);
        }

        public ConnectionView()
        {

            InitializeComponent();

            MCU_CAN_AV.Devices.IDevice.LogUpdater.Subscribe(_ => { 
                Dispatcher.UIThread.Post(() =>
                {
                    Tb_log.Text += $"{_} \n";
                    Scroll_log.ScrollToEnd();
                    Tb_cnct_msg.Text += $"{_} \n";
                    Scroll.ScrollToEnd();
                });
            });


            Lb_ConType.ItemsSource = new string[] { "USB-CAN-B", "Modbus TCP", "Modbus RTU" };
            Lb_ConType.SelectedIndex = 0;
            Lb_ConType.DropDownClosed += (_, __) =>
            {

                switch (Lb_ConType.SelectedIndex)
                {
                    case 0:
                        stp_can_usb_b.IsVisible = true;
                        stp_ModbusTCP.IsVisible = false;
                        stp_ModbusRTU.IsVisible = false;
                        break;
                    case 1:
                        stp_can_usb_b.IsVisible = false;
                        stp_ModbusTCP.IsVisible = true;
                        stp_ModbusRTU.IsVisible = false;
                        break;
                    case 2:
                        stp_can_usb_b.IsVisible = false;
                        stp_ModbusTCP.IsVisible = false;
                        stp_ModbusRTU.IsVisible = true;
                        break;

                }

            };

        }
    }
}
