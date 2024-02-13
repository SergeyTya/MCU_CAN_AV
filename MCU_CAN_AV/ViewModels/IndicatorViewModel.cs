using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData.Binding;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    public partial class IndicatorViewModel: ObservableRecipient, IRecipient<ConnectionState>
    {
        public IndicatorViewModel() {
            Messenger.RegisterAll(this);
        }

        [ObservableProperty]
        ObservableCollection<IndicatorTemplate> _indicatorsList = new();

        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected) {
                foreach (var item in IDevice.GetInstnce().DeviceDescription) {
                    IndicatorsList.Add(new IndicatorTemplate(item));
                }
            };
        }
    }

    public partial class IndicatorTemplate: ObservableObject {

        [ObservableProperty]
        public string _name;
        [ObservableProperty]
        public IObservable<double> _value;
        [ObservableProperty]
        public bool _isReadWrite;

        public IndicatorTemplate(IDeviceParameter item) {
            string unit = item.Unit is null ? "" : $" ,{item.Unit}";
            Name = $" {item.Name}{unit}";
            Value = item.Value;
            IsReadWrite = item.IsReadWrite;
        }


        [RelayCommand]
        public void ClickItem()
        {
            Debug.WriteLine("sad");
        }
    }
}
