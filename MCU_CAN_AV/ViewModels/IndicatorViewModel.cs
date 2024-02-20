using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.Views;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;


namespace MCU_CAN_AV.ViewModels
{
    public partial class IndicatorViewModel : ObservableRecipient, IRecipient<ConnectionState>
    {
      
        public IndicatorViewModel()
        {
            Messenger.RegisterAll(this);
        }

        [ObservableProperty]
        ObservableCollection<IndicatorTemplate>? _indicatorsList;

        public void Receive(ConnectionState message)
        {
           
            if (message.state == ConnectionState.State.Connected)
            {
                IndicatorsList = new();
                foreach (var item in IDevice.GetInstnce().DeviceDescription)
                {
                    IndicatorsList.Add(new IndicatorTemplate(item));
                }
            };

            if (message.state == ConnectionState.State.Disconnected) {
                if (IndicatorsList != null) {

                    foreach (var item in IndicatorsList)
                    {
                        item.Dispose();
                    }
                    IndicatorsList?.Clear();

                }
                
            }
        }
    }

    public partial class IndicatorTemplate : ObservableObject, IDisposable
    {
        Logger2Window Logger;

        [ObservableProperty]
        public string _name = "no name";

        [ObservableProperty]
        public bool _isReadWrite = true;

        [ObservableProperty]
        public string _value = "0";

        [ObservableProperty]
        public SolidColorBrush _indicatorColor;

        public IDisposable? disposable;

        bool _isCombo = false;

        public IndicatorTemplate(IDeviceParameter Item)
        {
            
            string unit = Item.Unit is null ? "" : $", {Item.Unit}";
            Name = $"{Item.Name}{unit}";
            IsReadWrite = Item.IsReadWrite;
            Dispatcher.UIThread.Post(() => IndicatorColor = new(Avalonia.Media.Colors.Black, 0.2));


            disposable = Item.Value.Subscribe((_) =>
            {
                if (Item.IsReadWrite) return;

                Dispatcher.UIThread.Post(() =>
                {
                    if (Item.Options != null && Item.Options.Count > 0 && (int)_ < Item.Options.Count)
                    {
                        Value = Item.Options[(int)_][0];
                        _isCombo = true;
                        return;
                    }

                    if (Item.Max != 0 && _ > Item.Max) { IndicatorColor.Color = Avalonia.Media.Colors.Red; }
                    else
                    if (Item.Min != 0 && _ < Item.Min) { IndicatorColor.Color = Avalonia.Media.Colors.Blue; }
                    else
                    { IndicatorColor.Color = Avalonia.Media.Colors.Green; }

                    if (Math.Abs(_) > 0.001 || _ == 0.0 )
                    {
                        Value = _.ToString("#0.0##");
                    }
                    else {
                        Value = _.ToString("E2");
                    }
                });
            });
        }


        [RelayCommand]
        public void ClickItem()
        {
            if (_isCombo) return;
            if (Logger!=null && Logger.Is_Alive)
            {
                Logger.WindowState = WindowState.Minimized;
                Logger.WindowState = WindowState.Normal;
                return;
            }

            var binding = new Binding { 
                Source= this,
                Path = nameof(this.Value)
            
            };

            Logger = new Logger2Window(Name)
            {
                [!Logger2Window.InputValueProperty] = binding
               
            };

            Logger.Show();
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
                Logger?.Close();
                // Освобождаем управляемые ресурсы
            }
            // освобождаем неуправляемые объекты
            disposed = true;
        }

        ~IndicatorTemplate() {
            Dispose(false);
        }
    }
}
    
