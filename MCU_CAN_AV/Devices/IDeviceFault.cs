using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices
{
    public interface IDeviceFault
    {
        public string ID { get; }
        public string Name { get; }
    }
}
