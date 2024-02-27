using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Cryptography;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.utils;
using Splat;


namespace MCU_CAN_AV.ViewModels
{
    public partial class MainView3Model : ObservableRecipient, IRecipient<ConnectionState>, IEnableLogger
    {
        [ObservableProperty]
        public ObservableCollection<LogItem> _logs = new();

        IDisposable? disposable_log;
        public MainView3Model()
        {
            Messenger.RegisterAll(this);
        }

        [ObservableProperty]
        public int _error_cnt;

        [ObservableProperty]
        public string _deviceName = "no device name";

        [ObservableProperty]
        string _logText = "";

        [ObservableProperty]
        public ObservableCollection<IDeviceParameter> _deviceParameters;

        [RelayCommand]
        void LogClear() {
            Logs.Clear();
        }

        [RelayCommand]
        void LogSave()
        {
            //TODO
        }

        IDisposable disposable_errcnt;


        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {

                DeviceName = IDevice.Current.Name;

                disposable_errcnt?.Dispose();
                disposable_errcnt = IDevice.Current.Connection_errors_cnt.Subscribe((_) => Error_cnt = _);

                connected();
            }

            if (message.state == ConnectionState.State.Init)
            {
                disposable_log?.Dispose();
                var logProvider = Locator.Current.GetService<ILogProvider>();
                disposable_log = logProvider?.GetObservable.Subscribe(
                    (_) => {
                        Dispatcher.UIThread.Post(() => {
                            var tmp = new LogItem { Text = _, TextColor = new SolidColorBrush(Colors.Gray) };
                            if(_.Contains("Error")) tmp.TextColor.Color = Colors.Red;
                            Logs.Add(tmp);
                        });
                    });
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

    public partial class LogItem : ObservableObject {
        [ObservableProperty]
        string _text;

        [ObservableProperty]
        public SolidColorBrush _textColor;
    }

}

