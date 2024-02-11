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


        
       
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        private uint _device_id = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        //[Range(100, 200, ErrorMessage = "Not valid value")]
        [ValidateAge2(200,100)]
        private uint _polling_interval = 100;
       


        [ObservableProperty]
        private uint _can_ID = 0;

        [ObservableProperty]
        private uint _baudrate = 500000;


        [ObservableProperty]
        private uint _canmask= 0;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        string _serverName = "localhost";
       

        [ObservableProperty]
        private uint _serverPort = 8888;


        [ObservableProperty]
        public bool _isControlEnabled = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClickConnectCommand))]
        string _comName = "COM1";

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
                _CANType = (CANType) Connection_Type,
                _PollInterval_ms = Polling_interval,
                _devind = Device_id,
                _canind = Can_ID,
                _Baudrate = Baudrate,
                _Mask = Canmask,
                server_name = ServerName,
                server_port = ServerPort,
                com_name = ComName
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
                && (Device_id > 0)
                && (Polling_interval > 0);

            return ret_val;
        }

    }

    public sealed class ValidateAge2Attribute : ValidationAttribute
    {
    //Attributes MaxAge and MinAge will be assigned by characteristic parameters
    public uint MaxAge { get; }
        public uint MinAge { get; }
        public ValidateAge2Attribute(uint maxAge, uint minAge)
        {
            MaxAge = maxAge;
            MinAge = minAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            uint age = (uint) value;
        
            if(age < MaxAge && age > MinAge) return ValidationResult.Success;

            
            return new($"The youngest {MinAge}, the largest {MaxAge}");
        }
    }
}
