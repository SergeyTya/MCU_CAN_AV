using Avalonia.Data;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices;
using ScottPlot.Drawing.Colormaps;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using static MCU_CAN_AV.Can.ICAN;

namespace MCU_CAN_AV.ViewModels
{

    internal partial class ConnectionViewModel : ObservableValidator
    {

        int LogRowCount = 0;

        [ObservableProperty]
        private string _logText = " ";

        partial void OnLogTextChanged(string? oldValue, string newValue)
        {
            // property changed event
            if(LogRowCount++ > 200 ) { LogRowCount = 0; _logText = ""; }
        }

        [ObservableProperty]
        private CANType _connection_Type = CANType.ModbusTCP;

        partial void OnConnection_TypeChanged(CANType value)
        {
            switch(value)
            {
                case CANType.ModbusTCP:
                    Visible_baudrate = false;
                    Visible_comName = false;
                    Visible_serverName = true;
                    Visible_serverPort = true;
                    Visible_can_ID = false;
                    Visible_canmask = false;

                    break;
                case CANType.CAN_USBCAN_B:
                    Visible_baudrate = true;
                    Visible_comName = false;
                    Visible_serverName = false;
                    Visible_serverPort = false;
                    Visible_can_ID = true;
                    Visible_canmask = true;
                    break;
                case CANType.ModbusRTU:
                    Visible_baudrate = true;
                    Visible_comName = false;
                    Visible_serverName = false;
                    Visible_serverPort = false;
                    Visible_can_ID = false;
                    Visible_canmask = false;
                    break;
                case CANType.Dummy:
                    Visible_baudrate = true;
                    Visible_comName = true;
                    Visible_serverName = true;
                    Visible_serverPort = true;
                    Visible_can_ID = true;
                    Visible_canmask = true;
                    break;

            }
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        private bool _form_valid = true;

        [ObservableProperty]
        [ValidateUint(nameof(Form_valid))]
        private string _device_id = "1";
       
        [ObservableProperty]
        [ValidateUint(nameof(Form_valid), check_lim = true, min = 10, max = 1000)]
        private string _polling_interval = "100";
     
        [ObservableProperty]
        [ValidateUint(nameof(Form_valid))]
        private string _can_ID = "0";
        [ObservableProperty]
        bool visible_can_ID = true;

        [ObservableProperty]
        [ValidateUint(nameof(Form_valid))]
        private string _baudrate = "500000";
        [ObservableProperty]
        bool visible_baudrate = true;

        [ObservableProperty]
        [ValidateUint(nameof(Form_valid))]
        private string _canmask = "0";
        [ObservableProperty]
        bool visible_canmask = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        string _serverName = "localhost";
        [ObservableProperty]
        bool visible_serverName = true;

        [ObservableProperty]
        [ValidateUint(nameof(Form_valid))]
        private string _serverPort = "8888";
        [ObservableProperty]
        bool visible_serverPort = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        string _comName = "COM1";
        [ObservableProperty]
        bool visible_comName = true;

        [ObservableProperty]
        public bool _isControlEnabled = true;


        [ObservableProperty]
        bool _IsMsgVisible = false;

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task ClickConnect()
        {
            // Relay command blocking button until it executing

            bool isConnectionDone = true;

            IsMsgVisible = true;

            var init_structure = new ICAN.CANInitStruct
            {
                _CANType         = (CANType) Connection_Type,
                _PollInterval_ms = UInt32.Parse(Polling_interval),
                _devind          = UInt32.Parse(Device_id),
                _canind          = UInt32.Parse(Can_ID),
                _Baudrate        = UInt32.Parse(Baudrate),
                _Mask            = UInt32.Parse(Canmask),
                server_name      = ServerName,
                server_port      = UInt32.Parse(ServerPort),
                com_name         = ComName
            };

            IDevice.Create(
                DeviceType.EVMModbus,
                init_structure
                );

            IDevice.GetInstnce()?.LogUpdater.Subscribe(
                (_) =>
                {
                    LogText += $"{DateTime.Now}: {_} \n";
                });


            IDevice.GetInstnce()?.Init_stage.Subscribe(
               (_) =>
               {
                   isConnectionDone = _;
               });
            
            
            while (isConnectionDone == true)
            {

                await Task.Delay(100);
            }
        }

        private bool CanConnect() {
            bool ret_val =
                !string.IsNullOrEmpty(ServerName)
                && !string.IsNullOrEmpty(ComName)
                && Form_valid;
            return ret_val;
        }

    }

    public sealed class ValidateUintAttribute : ValidationAttribute
    {
        public ValidateUintAttribute(string propertyName, bool check_lim=false, uint max=0, uint min=0)
        {
            PropertyName = propertyName;
            this.min = min;
            this.max = max;
            this.check_lim = check_lim;
        }

        public string PropertyName { get; }
        public uint max, min;
        public bool check_lim;

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            uint res = 0;

            var instance = (ConnectionViewModel) validationContext.ObjectInstance;

            instance.GetType().GetProperty(PropertyName)?.SetValue(instance, false);

            if (UInt32.TryParse(value?.ToString(), out res)) {
               
                if (check_lim)
                {
                    if (res >= min && res <= max) {
                        instance.GetType().GetProperty(PropertyName)?.SetValue(instance, true);
                        return ValidationResult.Success;
                    }
                    return new($"Not in range from {min} to {max}");
                }
                else {
                    instance.GetType().GetProperty(PropertyName)?.SetValue(instance, true);
                    return ValidationResult.Success;
                }
            }

            return new($"Wrong number");
        }
    }
}
