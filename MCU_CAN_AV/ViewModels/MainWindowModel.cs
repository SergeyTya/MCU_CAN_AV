using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace MCU_CAN_AV.ViewModels
{
    public partial class MainWindowModel: ObservableRecipient, IRecipient<ConnectionState>
    {
        public MainWindowModel() {

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
