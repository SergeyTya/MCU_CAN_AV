using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class MainViewModel : ObservableRecipient, IRecipient<ConnectionState>, IEnableLogger
    {

        public MainViewModel() {
            Messenger.RegisterAll(this);
        }

        public void Receive(ConnectionState message)
        {
            
        }

        public void Closing() {
            Messenger.Send(new ConnectionState(ConnectionState.State.Disconnected));
        }

    }
}
