using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices
{
    public struct DeviceState
    {
        public static readonly string Run = "Run";
        public static readonly string Ready = "Ready";
        public static readonly string Fault = "Fault";
        public static readonly string NoConnect = "Not connected";
    }
}
