using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.utils;
using MCU_CAN_AV.Views;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels 
{

    internal partial class ControlPanelViewModel : ObservableRecipient, IRecipient<ConnectionState>, IEnableLogger
    {
        ScopeWindow? scopeWindow;

        public ControlPanelViewModel()
        {
            Messenger.RegisterAll(this);
            scopeWindow = new();
        }

        [ObservableProperty]
        public bool _deviceLogerState = false;

        [ObservableProperty]
        public bool _isVisible = false;

        [ObservableProperty]
        public bool _isConnected = false;

        [ObservableProperty]
        public bool _isControlsDisabled = true;

        partial void OnIsControlsDisabledChanged(bool value)
        {
            IDevice.Current.ControlEnabled = !value;
        }


        partial void OnDeviceLogerStateChanged(bool value)
        {
            var dataLogger = Locator.Current.GetService<IDataLogger>();
            if (value)
            {
                dataLogger?.start(IDevice.Current);
            }
            else
            {
                dataLogger?.close();
                scopeWindow?.Close();
            }
        }

        partial void OnIsConnectedChanged(bool value)
        {

        }

        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {
                IsVisible = true;
                IsConnected = true;
            }

            if (message.state == ConnectionState.State.Disconnected)
            {
                IsControlsDisabled = true;
                IsVisible = false;
            }
        }

        [RelayCommand]
        private void ClickScopeItem()
        {
            scopeWindow?.Close();
            scopeWindow = new();
            scopeWindow.Show();
        }
    }
}
