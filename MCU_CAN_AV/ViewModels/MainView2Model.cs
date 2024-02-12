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

        public void Receive(ConnectionState message)
        {
            ConnectionDone = message.state;
        }

    }
}
