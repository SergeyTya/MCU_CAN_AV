using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Can
{
    public partial interface ICAN
    {
        public struct CANInitStruct
        {

            public UInt32 _devind = 1;
            public UInt32 _canind = 0;
            public UInt32 _Baudrate = 500000;
            public UInt32 _RcvCode = 0;
            public UInt32 _Mask = 0xffffffff;
            public UInt32 _PollInterval_ms = 100;  /// Poll interval, ms

            public string server_name = "localhost";
            public uint server_port = 8888;

            public string com_name = "COM1";

            public CANInitStruct()
            {
            }
        }
    }

}
