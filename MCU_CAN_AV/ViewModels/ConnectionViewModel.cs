using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal class ConnectionViewModel:ViewModelBase
    {

        public void OnClickConnectCommand() {
            Debug.WriteLine("aass");
            MCU_CAN_AV.DeviceDescriprion.Shanghai.ShanghaiDeviceReader.Init();
        }
    }
}
