using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices
{
    public enum DeviceType
    {
        EVMModbus_TCP,
        EVMModbus_RTU,
        Shanghai_3in1_USBCAN,
        Shanghai_3in1_PCAN,
        EspiritekCAN,
        Dongfen_DGL1200_900,
        Dummy
    }
}
