using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MCU_CAN_AV.utils;
using Avalonia.Media;

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

                var _clear = () =>
                {
                    if (Rows != null)
                    {
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
                    foreach (var item in IDevice.Current.DeviceDescription)
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

        [RelayCommand]
        private async Task OpenFile()
        {
            // ErrorMessages?.Clear();
            try
            {
                var filesService = App.Current?.Services?.GetService<IFilesService>();
                if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

                var file = await filesService.OpenFileAsync();
                if (file is null) return;

                // Limit the text file to 1MB so that the demo wont lag.
                if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1)
                {
                    await using var readStream = await file.OpenReadAsync();
                    using var reader = new StreamReader(readStream);
                    var FileText = await reader.ReadToEndAsync();
                }
                else
                {
                    throw new Exception("File exceeded 1MB limit.");
                }
            }
            catch (Exception e)
            {
                //ErrorMessages?.Add(e.Message);
            }
        }


        [RelayCommand]
        private async Task SaveFile()
        {
            // ErrorMessages?.Clear();
            try
            {
                var filesService = App.Current?.Services?.GetService<IFilesService>();
                if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

                var file = await filesService.SaveFileAsync();
                if (file is null) return;


                //// Limit the text file to 1MB so that the demo wont lag.
                //if (FileText?.Length <= 1024 * 1024 * 1)
                //{
                //    var stream = new MemoryStream(Encoding.Default.GetBytes((string)FileText));
                //    await using var writeStream = await file.OpenWriteAsync();
                //    await stream.CopyToAsync(writeStream);
                //}
                //else
                //{
                //    throw new Exception("File exceeded 1MB limit.");
                //}
            }
            catch (Exception e)
            {
                // ErrorMessages?.Add(e.Message);
            }
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
        List<string> _optionsItems;


        List<List<string>> _optionsList;

        [ObservableProperty]
        public string _value;
        [ObservableProperty]
        public string _value_edt;
        [ObservableProperty]
        public int _optionSelected;

        [ObservableProperty]
        Action _write;

        [ObservableProperty]
        public SolidColorBrush _cellColor = new(Avalonia.Media.Colors.Black, opacity: 0.45);


        [RelayCommand]
        public void CellEndEdit()
        {

            if (IsComboCell)
            {
                foreach (var item in _optionsList)
                {
                    if (item[1] == Value)
                    {
                        OptionSelected = _optionsList.IndexOf(item);
                    }
                }

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

            if (Item.Options != null && Item.Options.Count > 0)
            {
                IsComboCell = true;
                _optionsList = Item.Options;
                OptionsItems = new List<string>();
                foreach (var item in _optionsList)
                {
                    if (item.Count > 0) OptionsItems.Add(item[0]);

                    double dev_val = 0;
                    Item.Value.Take(1).Subscribe(_ => dev_val = _);

                    if (item.Count > 1)
                    {
                        double opt_val = 0;
                        if (double.TryParse(item[1], out opt_val))
                        {
                            if (dev_val == opt_val)
                            {
                                OptionSelected = _optionsList.IndexOf(item);
                            }
                        }
                    }
                    else
                    {
                        item.Add(_optionsList.IndexOf(item).ToString());
                        if ((int)dev_val == _optionsList.IndexOf(item))
                        {
                            OptionSelected = (int)dev_val;
                        }
                    }
                }
            }
            else
            {
                IsComboCell = false;
            }

            Write = () =>
            {

                Dispatcher.UIThread.Post(() =>
                {

                    if (IsComboCell)
                    {
                        double val = 0;
                        if (Double.TryParse(_optionsList[OptionSelected][1], out val))
                        {
                            Item.writeValue(val);
                            CellColor.Color = Colors.Red;
                        }
                        return;
                    }
                    //
                    double db = 0;
                    Value_edt = Value_edt.Replace(',', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                    Value_edt = Value_edt.Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
                    if (Double.TryParse(Value_edt, out db))
                    {
                        Item.writeValue(db);
                        CellColor.Color = Colors.Red;
                    }
                    else
                    {
                        Value_edt = Value;
                    }

                  
                });
            };

            disposable = Item.Value.Subscribe((_) =>
            {
                Dispatcher.UIThread.Post(() =>
                {

                    string new_val = _.ToString("#0.##");

                    if (IsComboCell)
                    {
                        foreach (var item in _optionsList)
                        {
                            if (item[1] == ((int)_).ToString())
                            {
                                new_val = item[0];
                            }
                        }
                    }

                    if (new_val != Value)
                    {
                        Value = new_val;
                        Value_edt = new_val;

                        CellColor.Color = Colors.Yellow;

                     
                    }

                    CellColor.Color = Colors.Green;

                    Task.Run(async () =>
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                        Dispatcher.UIThread.Post(() => CellColor.Color = Colors.Black);
                    });
                });
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
                OptionsItems?.Clear();
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
