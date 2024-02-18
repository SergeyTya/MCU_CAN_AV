using Avalonia.Logging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData.Binding;
using MCU_CAN_AV.Devices;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    public partial class ParamTableViewModel : ObservableRecipient, IRecipient<ConnectionState>
    {
        [ObservableProperty]
        public ObservableCollection<RowTemplate>? _rows;

        public ParamTableViewModel()
        {

            Messenger.RegisterAll(this);
        }


        public void Receive(ConnectionState message)
        {
            Dispatcher.UIThread.Post(() =>
            {

                var _clear = () => {
                    if (Rows != null) {
                        foreach (var item in Rows)
                        {
                            item.Dispose();
                        }
                        Rows.Clear();
                    }
                };

                if (message.state == ConnectionState.State.Connected)
                {
                    // _clear();
                    Rows = new();
                    foreach (var item in IDevice.GetInstnce().DeviceDescription)
                    {
                        if (item.IsReadWrite) Rows.Add(new RowTemplate(item));
                    }
                };

                if (message.state == ConnectionState.State.Disconnected)
                {
                    _clear();
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

        [RelayCommand]
        public void CellEndEdit() {
            Debug.WriteLine("----------------------->");

            if (IsComboCell)
            {
                OptionSelected = Options.IndexOf(Value);
                return;
            }
            Value_edt = Value;

        }

        IDisposable disposable;

        public RowTemplate(IDeviceParameter Item)
        {

            if (!Item.IsReadWrite) return;

            Name = Item.Name;
            Id = Item.ID;
            Options = Item.Options;
            IsComboCell = (Item.Options != null) && (Item.Options.Count > 0);
            Write = () => {
                if (IsComboCell)
                {
                    Item.writeValue(OptionSelected);
                    return;
                }
                //
                double db = 0;
                Value_edt = Value_edt.Replace(',', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                Value_edt = Value_edt.Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                if (Double.TryParse(Value_edt, out db))
                {
                    Item.writeValue(db);
                }
                else {
                    Value_edt = Value;
                }
            };

            disposable = Item.Value.Subscribe((_) =>
            {
                string new_val = _.ToString("#0.##");

                if (IsComboCell)
                {
                    new_val = Options[(int)_];
                }

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
                disposable?.Dispose();
                Options?.Clear();
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
