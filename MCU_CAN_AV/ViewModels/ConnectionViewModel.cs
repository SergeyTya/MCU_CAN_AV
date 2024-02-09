using Avalonia.Data;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal class ConnectionViewModel:ViewModelBase
    {


        public ConnectionViewModel() {
          
        }

        bool _IsMsgVisible = false;
        public bool IsMsgVisible
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _IsMsgVisible, value);
            }
            get
            {
                return _IsMsgVisible;
            }
        }

        bool _IsConnEnabled = true;
        public bool IsConnEnabled
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _IsConnEnabled, value);
            }
            get
            {
                return _IsConnEnabled;
            }
        }

        public void OnClickConnectCommand()
        {
            MCU_CAN_AV.Devices.IDevice.Create(

                IDevice.DeviceType.EVMModbus,

                new ICAN.CANInitStruct
                {
                    _CANType = ICAN.CANType.ModbusTCP,
                    _devind = 1
                }

                );

            IsMsgVisible = true;
            IsConnEnabled = false;
            
        }

    }
}
