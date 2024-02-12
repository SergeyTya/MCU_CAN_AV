using System;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;

namespace MCU_CAN_AV.ViewModels
{
    public partial class MainView3Model : ObservableObject
    {
        IDisposable? disposable_log;
        public MainView3Model()
        {
            disposable_log?.Dispose();
            disposable_log = IDevice._LogUpdater.Subscribe((_) => LogText += $"{DateTime.Now}: {_} \n");
        }

        [ObservableProperty]
        string _logText = "";

    }

}

