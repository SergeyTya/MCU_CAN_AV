using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal interface IModbusTransport
    {
        public Task<string> getDevId();
        public Task<ushort[]> ReadHRsAsync(uint count);

        public Task<List<byte[]>> ReadRegistersInfoAsync();

        public Task<uint> ReadRegisterCount();

    }
}
