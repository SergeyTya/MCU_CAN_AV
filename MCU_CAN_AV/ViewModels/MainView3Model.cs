using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Avalonia.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.utils;
using Splat;


namespace MCU_CAN_AV.ViewModels
{
    public partial class MainView3Model : ObservableRecipient, IRecipient<ConnectionState>
    {
        IDisposable? disposable_log;
        public MainView3Model()
        {
            Messenger.RegisterAll(this);

            //  disposable_log = StaticLogger.Subscribe((_) => LogText += _);
   
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
                disposable_log?.Dispose();
                var logProvider = Locator.Current.GetService<ILogProvider>();
                disposable_log = logProvider?.GetObservable.Subscribe((_) => LogText += _);


                DeviceName = IDevice.Current.Name;

                disposable_errcnt?.Dispose();
                disposable_errcnt = IDevice.Current.Connection_errors_cnt.Subscribe((_) => Error_cnt = _ );

                connected();
            }
        }

        void connected() {
            IDevice? inst = IDevice.Current;
            DeviceParameters = inst.DeviceDescription;
        }


        private static IObservable<string?> ConsoleInput()
        {
            return
                Observable
                    .FromAsync(() => Console.In.ReadLineAsync())
                    .Repeat()
                    .Publish()
                    .RefCount()
                    .SubscribeOn(Scheduler.Default);
        }
    }

}

