using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class ButtonsControlViewModel: ObservableRecipient, IRecipient<ConnectionState>
    {
        [RelayCommand]
        public void onClickResetButton() {
            Messenger.Send(new ConnectionState(ConnectionState.State.Reset));
            IDevice.Current?.Reset(); 
        }
        [RelayCommand]
        public void onClickStartButton() => IDevice.Current?.Start();
        [RelayCommand]
        public void onClickStopButton() => IDevice.Current?.Stop();


        public ButtonsControlViewModel()
        {
            Messenger.RegisterAll(this);
        }


        public void Receive(ConnectionState message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (message.state == ConnectionState.State.Connected)
                {

                };

                if (message.state == ConnectionState.State.Disconnected)
                {
   
                }
            });
        }




    }
}
