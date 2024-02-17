using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{

    internal partial class FaultTableViewModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        ObservableCollection<IDeviceFault> _deviceFaults = new();

        [ObservableProperty]
        string _state;

        [ObservableProperty]
        public SolidColorBrush _indicatorColor = new(Avalonia.Media.Colors.Black);

        IDisposable _disposed;


        public FaultTableViewModel() {
            Messenger.RegisterAll(this);
        }


        public void Receive(ConnectionState message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (message.state == ConnectionState.State.Connected)
                {
                    DeviceFaults = IDevice.GetInstnce().DeviceFaults;

                    _disposed = IDevice.GetInstnce().State.Subscribe((_) => {
                        State = _;
                        if (_ == DeviceState.Run) { IndicatorColor.Color = Colors.Green; }
                        if (_ == DeviceState.Error) { IndicatorColor.Color = Colors.Red; }
                        if (_ == DeviceState.Ready) { IndicatorColor.Color = Colors.Blue; }
                        if (_ == DeviceState.NoConnect) { IndicatorColor.Color = Colors.DimGray; }
                    });
                };

                if (message.state == ConnectionState.State.Disconnected)
                {
                    DeviceFaults.Clear();
                    _disposed?.Dispose();
                }
            });
        }

    }

}
