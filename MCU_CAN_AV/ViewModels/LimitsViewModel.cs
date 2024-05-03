using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using DynamicData;
using MCU_CAN_AV.Devices;
using Newtonsoft.Json.Linq;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using AsyncSocketTest;
using System.Collections;
using System.Data.Common;

namespace MCU_CAN_AV.ViewModels
{
    internal partial class LimitsViewModel : ObservableRecipient, IEnableLogger, IRecipient<ConnectionState>
    {
        System.Timers.Timer? timer;
        ServerModbusTCP ? RxConnection;

        private void ReadEvmState() { 
            

            
        }

        public LimitsViewModel() {
            Messenger.RegisterAll(this);
        }    

        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {
                this.Log().Info("LimitsViewModel Connected");

                timer = new System.Timers.Timer(100);
                timer.Elapsed += async (_, __) =>
                {

                    if (EVMblk_enable == false) {
                        EVM_State = "Disabled";
                        timer.Interval = 5000;
                        return;
                    } 


                    try {
                        if (RxConnection == null) {
                            RxConnection = new ServerModbusTCP("localhost", 8888);
                        }

                        if (!RxConnection.Connected)
                        {
                            RxConnection = new ServerModbusTCP("localhost", 8888);
                        }

                       

                        var res = await RxConnection.ReadHoldingsAsync(1, 1, 1);

                        BitArray bits = new BitArray(BitConverter.GetBytes(res[0]));
                        
                        if (bits[0] == true) {
                            timer.Interval = 50;
                            EVM_State = "Run";
                        }

                        if (bits[1] == true)
                        {
                            EVM_State = "Rdy";
                            timer.Interval = 2000;
                            IDevice.Current.Stop();
                        }

                        if (bits[2] == true)
                        {
                            EVM_State = "Fault";
                            timer.Interval = 2000;
                            IDevice.Current.Stop();
                        }

                        
                    }
                    catch (Exception ex) {
                        RxConnection = null;
                      //  this.Log().Warn(ex.Message.ToString());
                        EVM_State = "not connected";
                        timer.Interval = 1000;
                        IDevice.Current.Stop();
                    }
                   
                };

                timer?.Start();

            }

            if (message.state == ConnectionState.State.Disconnected)
            {
                RxConnection?.close();
                DisposableSpeed?.Dispose();
                timer?.Stop();
                timer?.Dispose();
            }
        }

        [ObservableProperty]
     //   [ValidateUint2()]
        private string _voltageLim = "0";

        [ObservableProperty]
        private bool _voltageLim_enable = false;

        [ObservableProperty]
    //    [ValidateUint2()]
        private string _speedLim = "0";

        [ObservableProperty]
        private bool _speedLim_enable = false;

        [ObservableProperty]
        private string _eVM_State = "not connected";

        [ObservableProperty]
        private bool _eVMblk_enable = false;

        IDisposable? DisposableSpeed;


    }

    public sealed class ValidateUint2Attribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            double res;
            if (Double.TryParse(value?.ToString(), out res))
            {
                return ValidationResult.Success;
            }

            return new($"Wrong number");
        }
    }

}
