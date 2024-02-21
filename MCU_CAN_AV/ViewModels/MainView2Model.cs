using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class MainView2Model: ObservableRecipient, IRecipient<ConnectionState>
    {

        public MainView2Model()
        {
            Messenger.RegisterAll(this);
        }

        [ObservableProperty]
        bool _connectionDone = false;

        [ObservableProperty]
        string _logText = "sdcdsc";

        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected) {
                ConnectionDone = true;
            }

            if (message.state == ConnectionState.State.Disconnected)
            {
                ConnectionDone = false;
            }
        }

    }
}
