using Avalonia.Logging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData.Binding;
using MCU_CAN_AV.Devices;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    public partial class ParamTableViewModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        public ObservableCollection<RowTemplate> _rows = new();

        public ParamTableViewModel()
        {

            Messenger.RegisterAll(this);
        }


        public void Receive(ConnectionState message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (message.state == ConnectionState.State.Connected)
                {
                    foreach (var item in IDevice.GetInstnce().DeviceDescription)
                    {
                        if (item.IsReadWrite) Rows.Add(new RowTemplate(item));
                    }
                };

                if (message.state == ConnectionState.State.Disconnected)
                {

                    foreach (var item in Rows)
                    {
                        item.Dispose();
                    }
                    Rows.Clear();

                }
            });
        }
    }


    public partial class RowTemplate : ObservableObject, IDisposable
    {
        [ObservableProperty]
        bool _isComboCell = false;
        [ObservableProperty]
        public string _name = "noName";
        [ObservableProperty]
        public string _id = "ID";
        [ObservableProperty]
        List<string> _options;
        [ObservableProperty]
        public string _value;
        [ObservableProperty]
        public string _value_edt;
        [ObservableProperty]
        public int _optionSelected;

        [ObservableProperty]
        Action _write;

        IDisposable disposable;

        public RowTemplate(IDeviceParameter Item)
        {

            if (!Item.IsReadWrite) return;

            Name = Item.Name;
            Id = Item.ID;
            Options = Item.Options;
            IsComboCell = (Item.Options != null) && (Item.Options.Count > 0);
            Write = () => { 
                Item.writeValue(6666);
            };

            disposable = Item.Value.Subscribe((_) =>
            {
                if (IsComboCell)
                {
                    OptionSelected = (int)_;
                    return;
                }

                string new_val = _.ToString("#0.##");
                if (new_val != Value)
                {
                    Value = new_val;
                    Value_edt = new_val;
                }

            });
        }

        private bool disposed = false;
        public void Dispose()
        {

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                disposable.Dispose();
                Options.Clear();
            }
            // освобождаем неуправляемые объекты
            disposed = true;
        }

        ~RowTemplate()
        {
            Dispose(false);
        }
    }
}
