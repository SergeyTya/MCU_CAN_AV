using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.utils;

namespace MCU_CAN_AV.ViewModels
{
    public partial class MainView3Model : ObservableRecipient, IRecipient<ConnectionState>
    {
        IDisposable? disposable_log;
        public MainView3Model()
        {
            Messenger.RegisterAll(this);
            disposable_log?.Dispose();
            disposable_log = StaticLogger.Subscribe((_) => LogText += _);
        }

        [ObservableProperty]
        public int _error_cnt;

        [ObservableProperty]
        public string _deviceName = "no device name";

        [ObservableProperty]
        string _logText = "";

        [ObservableProperty]
        public ObservableCollection<IDeviceParameter> _deviceParameters;

        IDisposable disposable_errcnt;


        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {
                DeviceName = IDevice.GetInstnce().Name;

                disposable_errcnt?.Dispose();
                disposable_errcnt = IDevice.GetInstnce().Connection_errors_cnt.Subscribe((_) => Error_cnt = _ );

                connected();
            }
        }

        void connected() {
            IDevice? inst = IDevice.GetInstnce();
            DeviceParameters = inst.DeviceDescription;
        }
    }

}

