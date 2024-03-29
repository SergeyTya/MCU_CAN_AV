﻿using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{

    internal partial class FaultTableViewModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        ObservableCollection<FaultRecord> _deviceFaults;

        [ObservableProperty]
        string _state;

        [ObservableProperty]
        public SolidColorBrush _indicatorColor = new(Avalonia.Media.Colors.Black);

        IDisposable _disposed;
        IDisposable disposable;

        public FaultTableViewModel() {
            Messenger.RegisterAll(this);
            DeviceFaults = new ObservableCollection<FaultRecord>();

        }

        public void Receive(ConnectionState message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (message.state == ConnectionState.State.Connected)
                {
                    //DeviceFaults = IDevice.GetInstnce().DeviceFaults;

                    _disposed = IDevice.Current.State.Subscribe((_) => {
                        Dispatcher.UIThread.Post(() =>
                        {
                            State = _;
                            if (_ == DeviceState.Run) { IndicatorColor.Color = Colors.Green; }
                            if (_ == DeviceState.Fault) { IndicatorColor.Color = Colors.DarkRed; }
                            if (_ == DeviceState.Ready) { IndicatorColor.Color = Colors.DarkBlue; }
                            if (_ == DeviceState.NoConnect) { IndicatorColor.Color = Colors.DimGray; }
                        });
                    });

                    disposable?.Dispose();
                    disposable = IDevice.Current.DeviceFaults.Subscribe((_) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            bool new_fault = true;
                            foreach (var el in DeviceFaults)
                            {
                                if (el == null) continue;
                                if (el.Name == _.Name)
                                {
                                    el.FaultColor.Color = Colors.Red;
                                    new_fault = false;

                                }
                            }

                            if (new_fault)
                            {
                                DeviceFaults.Insert(0, new FaultRecord(_.Name));
                            }
                        });

                    });

                };

                if (message.state == ConnectionState.State.Disconnected)
                {
                    DeviceFaults.Clear();
                    _disposed?.Dispose();
                    disposable?.Dispose();
                }

                if (message.state == ConnectionState.State.Reset)
                {
                    foreach (var el in DeviceFaults) { 
                        el.Dispose();
                    }
                    DeviceFaults.Clear();
                }
            });
        }

    }

    public partial class FaultRecord : ObservableObject, IDisposable {
        
        bool disposed = false;
        public FaultRecord(string Name) {

            this.Name = Name;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Dispatcher.UIThread.Post(() =>
                    {
                        FaultColor = new SolidColorBrush(Colors.Red);
                        if (expired++ == 3)
                        {
                            if(FaultColor!=null) FaultColor.Color = Colors.Gray;
                        }   

                    });
                    if (disposed) break;
                    
                }
            });
        }

        int expired = 0;

        [ObservableProperty]
        string _name;

        [ObservableProperty]
        SolidColorBrush _faultColor = new SolidColorBrush(Colors.Red);

        partial void OnFaultColorChanged(SolidColorBrush value)
        {
            if (value.Color == Colors.Red) {
                expired = 0;
            }
        }

        public void Dispose()
        {
            disposed = true;
        }
    }

}
