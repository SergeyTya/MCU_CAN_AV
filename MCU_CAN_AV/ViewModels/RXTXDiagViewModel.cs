using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class RXTXDiagViewModel: ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        public ObservableCollection<RxTxRowTemplate>? _rxData;

        [ObservableProperty]
        public ObservableCollection<RxTxRowTemplate>? _txData;

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

                        string hex = BitConverter.ToString(data.data);
                        var tmp_row = new RxTxRowTemplate { Id = data.id, Value = hex };

                        bool new_item = true;
                        foreach (var item in tbl)
                        {
                            if (item.Id == data.id)
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

        [ObservableProperty]
        uint _id;

        [ObservableProperty]
        string? _value;

        [ObservableProperty]
        int _counter;
    }

}
