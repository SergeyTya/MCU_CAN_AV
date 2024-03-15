
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.utils;
using Microsoft.Extensions.Options;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static MCU_CAN_AV.Can.ICAN;
using static MCU_CAN_AV.ViewModels.ConnectionState;

namespace MCU_CAN_AV.ViewModels
{
    public record class ConnectionState(State state) {
        public enum State { 
            Init,
            Connected,
            Disconnected,
            Reset
        }
    };

    internal partial class ConnectionViewModel : ObservableRecipient, IEnableLogger, IRecipient<ConnectionState>
    {
        
     

        public static CANInitStruct InitStruct = new CANInitStruct();

     

        [ObservableProperty]
        public ObservableCollection<ParameterField> _parameterItems = new() {

            new ParameterField {
                Label = "Polling interval",
                Name = nameof(InitStruct._PollInterval_ms),
                TextInput = "100",
                onNext_uint = (_) => {
                    InitStruct._PollInterval_ms=_;
                }
            },

            new ParameterField {
                Label = "Device Id",
                Name = nameof(InitStruct._devind),
                TextInput = "1",
                onNext_uint = (_) => {
                    InitStruct._devind=_;
                }
            },

            new ParameterField {
                Label = "Baudrate",
                Name = nameof(InitStruct._Baudrate),
                TextInput = "500000",
                onNext_uint = (_) => {
                    InitStruct._Baudrate=_;
                }
            },

            new ParameterField {
                Label = "Can ID",
                Name = nameof(InitStruct._canind),
                TextInput = "0",
                onNext_uint = (_) => {
                    InitStruct._canind=_;
                }
            },

            new ParameterField {
                Label = "Can Mask",
                Name = nameof(InitStruct._Mask),
                TextInput = "0",
                onNext_uint = (_) => {
                    InitStruct._Mask=_;
                }
            },

            new ParameterField {
                Label = "TCP Server Name",
                Name = nameof(InitStruct.server_name),
                TextInput = "localhost",
                Validate = false,
                onNext_string = (_) => {
                    InitStruct.server_name=_;
                }
            },

            new ParameterField {
                 Label = "TCP Server Port",
                 Name = nameof(InitStruct.server_port),
                 TextInput = "8888",
                 onNext_uint = (_) => {
                    InitStruct.server_port=_;
                 }
            },

            new ParameterField {
                 Label = "COM Port Name",
                 Name = nameof(InitStruct.com_name),
                 TextInput = "COM1",
                  Validate = true,
                 onNext_string = (_) => {
                    InitStruct.com_name=_;
                 }
            },

        };

        int LogRowCount = 0;

        [ObservableProperty]
        public List<String> _deviceAvalible = new List<String>() {nameof(DeviceType.EVMModbus), nameof(DeviceType.ShanghaiCAN), nameof(DeviceType.EspiritekCAN), nameof(DeviceType.Dummy) };

        [ObservableProperty]
        private string _logText = " ";

        [ObservableProperty]
        private DeviceType _deviceSelected;

        public ConnectionViewModel()
        {

            foreach (var item in ParameterItems)
            {
                item.disposable?.Dispose();
                item.disposable = item.Valid.Subscribe((_) => ClickConnectCommand.NotifyCanExecuteChanged());
            }

            DeviceSelected = DeviceType.Dummy;
            DeviceSelected = DeviceType.EVMModbus;
        }

        partial void OnLogTextChanged(string? oldValue, string newValue)
        {
            // property changed event
            if (LogRowCount++ > 100) { LogRowCount = 0; _logText = "";  }
        }

       
        partial void OnDeviceSelectedChanged(DeviceType value)
        {
            foreach (var item in ParameterItems)
            {
                // Grabli
                if (item.Name is nameof(InitStruct._Baudrate))
                {
                    item.IsVisible = value == DeviceType.EVMModbus || value == DeviceType.ShanghaiCAN || value == DeviceType.EspiritekCAN || value == DeviceType.Dummy;
                }
                if (item.Name is nameof(InitStruct.com_name))
                {
                    item.IsVisible = value == DeviceType.EVMModbus || value == DeviceType.Dummy;

                    if (value == DeviceType.EVMModbus)
                    {
                       
                        item.Options = SerialPort.GetPortNames(); 

                    }
                }
                if (item.Name is nameof(InitStruct._canind))
                {
                    item.IsVisible = value == DeviceType.ShanghaiCAN || value == DeviceType.EspiritekCAN || value == DeviceType.Dummy;
                }
                if (item.Name is nameof(InitStruct._Mask))
                {
                    item.IsVisible = value == DeviceType.ShanghaiCAN || value == DeviceType.EspiritekCAN || value == DeviceType.Dummy; 
                }
                if (item.Name is nameof(InitStruct.server_name))
                {
                    item.IsVisible = value == DeviceType.EVMModbus || value == DeviceType.Dummy;
                }
                if (item.Name is nameof(InitStruct.server_port))
                {
                    item.IsVisible = value == DeviceType.EVMModbus || value == DeviceType.Dummy;
                }
            }

        
        }


        [ObservableProperty]
        public bool _isControlEnabled = true;


        [ObservableProperty]
        bool _IsMsgVisible = false;


        IDisposable? disposable_logDevice;
        IDisposable? disposable_log;
        IDisposable? disposable_init;
        bool isConnectionDone = true;

        [RelayCommand]
        private void ClickDisconnect()
        {
            IsMsgVisible = false;
            disposable_log?.Dispose();
            disposable_init?.Dispose();
            isConnectionDone = false;
            Messenger.Send(new ConnectionState(ConnectionState.State.Disconnected));
            IDevice.Dispose();

        }

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task ClickConnect()
        {

            Messenger.Send(new ConnectionState(ConnectionState.State.Init));

            LogText = "";
           
            // Relay command blocking button until it executing
            IsMsgVisible = true;

            // Subscribe log window to logger eventualy
            disposable_log?.Dispose();

            var logProvider = Locator.Current.GetService<ILogProvider>();
            disposable_log = logProvider?.GetObservable.Subscribe((_) =>
            {
               LogText += _ ;
            });

            // Print current setups
            foreach (var item in ParameterItems)
            {
                //  item.disposable?.Dispose();
               this.Log().Info( $"  {item.Name}= {item.TextInput}" );
            }

            // Create user requested device
            IDevice.Create( DeviceSelected, InitStruct );

            // Wait for connection done    
            disposable_init = IDevice.Current?.Init_stage.Subscribe(
               (_) =>
               {
                   isConnectionDone = _;
                   if (!_) {
                       Messenger.Send(new ConnectionState(ConnectionState.State.Connected));
                       disposable_init?.Dispose();
                       disposable_log?.Dispose();  
                   }
                   
               });

        }

        private bool CanConnect()
        {
            bool res = true;
            foreach (var item in ParameterItems)
            {
                res &= item.IsValid;
            }
            return res;
        }

        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {
                IsMsgVisible = false;
            }
        }
    }


    public partial class ParameterField :  ObservableValidator
    {
        public ParameterField() {
           OnTextInputChanged(_textInput);
            
        }

        public string? Name { get; set; }

        [ObservableProperty]
        public string[]? _options = null;

        [ObservableProperty]
        public bool _validate = true;

        [ObservableProperty]
        string _label = "no name";

        [ObservableProperty]
        [ValidateUint(nameof(IsValid), nameof(Validate), check_lim: false, max :0, min :0)]
        string _textInput = "";

        [ObservableProperty]
        uint _value = 0;

        partial void OnValueChanged(uint value)
        {
            onNext_uint(Value);
        }

        partial void OnTextInputChanged(string value)
        {
            uint res = 0;
            if (UInt32.TryParse(TextInput, out res))
            {
                Value = res;
            }

            onNext_string(value);
        }

        [ObservableProperty]
        bool _isVisible = true;

        [ObservableProperty]
        bool _isValid = true;

        partial void OnIsValidChanged(bool value)
        {
            // Kostyli!
            Valid.OnNext(value);
        }

        public Subject<bool> Valid = new();
        public IDisposable? disposable = null;

        public Action<uint> onNext_uint = (_) => { };

        public Action<string> onNext_string = (_) => { };



    }


    public sealed class ValidateUintAttribute : ValidationAttribute
    {
        public ValidateUintAttribute(
            string propertyName,
            string EnableFieldName,
            bool check_lim = false,
            uint max = 0,
            uint min = 0
         
        )
        {
            PropertyName = propertyName;
            this.EnableFieldName = EnableFieldName;
            this.min = min;
            this.max = max;
            this.check_lim = check_lim;
           
        }

        public string PropertyName { get; }
        public string EnableFieldName { get;  }
        public uint   max, min;
        public bool   check_lim;
      
       
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            uint res = 0;

            var instance = (ParameterField)validationContext.ObjectInstance;
            var validate = instance.GetType().GetProperty(EnableFieldName)?.GetValue(instance);

            if(validate != null)
            {
                if ((bool)validate == false)
                {
                    instance.GetType().GetProperty(PropertyName)?.SetValue(instance, true);
                    return ValidationResult.Success;
                }

            }

            if (instance.Options != null) {

                var tmp = value?.ToString();
                if (tmp != null)
                {
                    if (instance.Options.IndexOf(tmp) != -1)
                    {
                        return ValidationResult.Success;
                    }
                    else {

                        return new( $"Avalible ports: {string.Join(" ",instance.Options)}");
                    }
                }
            }

            instance.GetType().GetProperty(PropertyName)?.SetValue(instance, false);

            if (UInt32.TryParse(value?.ToString(), out res))
            {

                if (check_lim)
                {
                    if (res >= min && res <= max)
                    {
                        instance.GetType().GetProperty(PropertyName)?.SetValue(instance, true);
                        return ValidationResult.Success;
                    }
                    return new($"Not in range from {min} to {max}");
                }
                else
                {
                    instance.GetType().GetProperty(PropertyName)?.SetValue(instance, true);
                    return ValidationResult.Success;
                }
            }

            return new($"Wrong number");
        }
    }
}
