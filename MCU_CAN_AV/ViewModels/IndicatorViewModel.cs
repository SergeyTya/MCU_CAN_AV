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
            IndicatorsList = new();
        }

        [ObservableProperty]
        ObservableCollection<IndicatorTemplate>? _indicatorsList;

        public void Receive(ConnectionState message)
        {
           
            if (message.state == ConnectionState.State.Connected)
            {
                var tmp = IDevice.Current.DeviceDescription;
                foreach (var item in tmp)
                {
                    IndicatorsList?.Add(new IndicatorTemplate(item));
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
        private LoggerWindow? _logger;

        [ObservableProperty]
        string _info = "";

        [ObservableProperty]
        private string _name = "no name";

        [ObservableProperty]
        private bool _isReadWrite = true;

        [ObservableProperty]
        private string _value = "0";

        [ObservableProperty]
        private SolidColorBrush? _indicatorColor;

        private readonly IDisposable? _disposable;

        private bool _isCombo = false;

        public IndicatorTemplate(IDeviceParameter item)
        {
            Info = $"ID = {item.ID}";
            var unit = (item.Unit == string.Empty) ? "" : $", {item.Unit}";
            Name = $"{item.Name}{unit}";
            IsReadWrite = item.IsReadWrite;
            Dispatcher.UIThread.Post(() => IndicatorColor = new(Avalonia.Media.Colors.Black, 0.2));

            _disposable = item.Value.Subscribe((_) =>
            {
                if (item.IsReadWrite) return;

                Dispatcher.UIThread.Post(() =>
                {
                    if (IndicatorColor == null) return;

                    if (item.Options != null && item.Options.Count > 0 && (int)_ < item.Options.Count)
                    {
                        Value = item.Options[(int)_][0];
                        _isCombo = true;
                        return;
                    }

                    if (item.Max != 0 && _ > item.Max) { IndicatorColor.Color = Avalonia.Media.Colors.Red; }
                    else
                    if (item.Min != 0 && _ < item.Min) { IndicatorColor.Color = Avalonia.Media.Colors.Blue; }
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
        private void ClickItem()
        {
            if (_isCombo) return;
            if (_logger!=null) if( _logger.IsAlive == true)
            {
                _logger.WindowState = WindowState.Minimized;
                _logger.WindowState = WindowState.Normal;
                return;
            }

            var binding = new Binding { 
                Source= this,
                Path = nameof(this.Value)
            
            };

            _logger = new LoggerWindow(Name)
            {
                [!LoggerWindow.InputValueProperty] = binding
               
            };

            _logger.Show();
        }

        private bool _disposed = false;
        public void Dispose()
        {
           
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _disposable?.Dispose();
                _logger?.Close();
                // Освобождаем управляемые ресурсы
            }
            // освобождаем неуправляемые объекты
            _disposed = true;
        }

        ~IndicatorTemplate() {
            Dispose(false);
        }
    }
}
    
