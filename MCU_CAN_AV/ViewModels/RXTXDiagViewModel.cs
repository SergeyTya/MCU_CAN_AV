﻿using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.ObjectModel;


namespace MCU_CAN_AV.ViewModels
{
    internal partial class RXTXDiagViewModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        public ObservableCollection<RxTxRowTemplate>? _rxData;

        [ObservableProperty]
        public ObservableCollection<RxTxRowTemplate>? _txData;

        [RelayCommand]
        void Clear()
        {
            RxData?.Clear();    
            TxData?.Clear();    
        }

        IDisposable RxDis;
        IDisposable TxDis;

        public RXTXDiagViewModel() {
            Messenger.RegisterAll(this);
        }

        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {
                RxData = new();
                TxData = new();

                Action<ICAN.RxTxCanData, ObservableCollection<RxTxRowTemplate> > action = (data, tbl) =>
                {
                    Dispatcher.UIThread.Post(() => {

                        if (data.Timeout) return;

                        string hex = BitConverter.ToString(data.data);
                        string ID  = $"0x{data.id.ToString("X4")}";
                        var tmp_row = new RxTxRowTemplate { Id = ID, Value = hex, id = data.id };

                        bool new_item = true;
                        foreach (var item in tbl)
                        {
                            if (item.id == data.id)
                            {
                                item.Value = hex;
                                item.Counter++;
                                new_item = false;
                            }
                        }

                        if (new_item)
                        {
                            tbl.Add(tmp_row);
                        }

                    });
                };

                RxDis = IDevice.Current.RxData.Subscribe((_) =>
                {
                    action(_, RxData);
                });

                TxDis = IDevice.Current.TxData.Subscribe((_) =>
                {
                    action(_, TxData);
                });
            };

            if (message.state == ConnectionState.State.Disconnected)
            {
                RxData?.Clear();
                TxData?.Clear();
                RxDis?.Dispose();
                TxDis?.Dispose();
            }
        }
    }

    internal partial class RxTxRowTemplate:ObservableObject {

        public uint id;

        [ObservableProperty]
        string? _id;

        [ObservableProperty]
        string? _value;

        [ObservableProperty]
        int _counter;
    }

}
