using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
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

namespace MCU_CAN_AV.ViewModels
{
    internal partial class LimitsViewModel : ObservableValidator, IEnableLogger, IRecipient<ConnectionState>
    {
        public void Receive(ConnectionState message)
        {
            if (message.state == ConnectionState.State.Connected)
            {
                DisposableSpeed = IDevice.Current.OutSpeed.Value.Subscribe((_) =>
                {
                    if (SpeedLim_enable)
                    {
                        double res;

                        if (Double.TryParse(SpeedLim, out res) == false) return;

                        if (Math.Abs(_) > res)
                        {
                            IDevice.Current.Stop();

                            ((Subject<IDeviceFault>)IDevice.Current.DeviceFaults).OnNext(new BaseDeviceFault("VISU_FLT: Speed"));
                        }
                    }
                });
            }

            if (message.state == ConnectionState.State.Disconnected)
            {
                DisposableSpeed?.Dispose();
            }
        }

        [ObservableProperty]
        [ValidateUint2()]
        private string _voltageLim = "0";

        [ObservableProperty]
        private bool _voltageLim_enable = false;

        [ObservableProperty]
        [ValidateUint2()]
        private string _speedLim = "0";

        [ObservableProperty]
        private bool _speedLim_enable = false;

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
