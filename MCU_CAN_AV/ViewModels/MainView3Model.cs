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

            //IDevice.GetInstnce().DeviceDescriprion.ToObservable().Subscribe(
            //   (_) => DeviceParametrs.Add(_));

            //IDevice.GetInstnce().DeviceDescriprion.CollectionChanged += DeviceDescriprion_CollectionChanged;
        }

      

        [ObservableProperty]
        string _logText = "";

        [ObservableProperty]
        public ObservableCollection<IDeviceParameter> _deviceParameters;

        partial void OnDeviceParametersChanged(ObservableCollection<IDeviceParameter> value)
        {
           // throw new NotImplementedException();
        }



        public void Receive(ConnectionState message)
        {
           if(message.state == ConnectionState.State.Connected) connected();
        }

        void connected() {
            IDevice? inst = IDevice.GetInstnce();
            DeviceParameters = inst.DeviceDescription;
        }
    }

}

