using AsyncSocketTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AsyncSocketTest.ServerModbusTCP;
using MCU_CAN_AV.utils;
using MCU_CAN_AV.Devices;
using System.Reactive.Linq;

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal class ModbusTCP : ICAN
    {
        uint reg_count = 0;
        string server_name = "localhost";
        uint server_port = 8888;
        uint modbus_id = 1;

        bool isOpen = false;

        public ModbusTCP(ICAN.CANInitStruct InitStruct)
        {
            //modbus_id = InitStruct._devind;
            //server_port = InitStruct.server_port;
            //server_name = InitStruct.server_name;

            //Func<Task<uint>> _ = async () =>
            //{
            //    try
            //    {
            //        return await ModbusTCP.ReadRegisterCount(server_name, server_port, modbus_id);
            //    }
            //    catch (ServerModbusTCPException ex)
            //    {
            //        IDevice.logUpdater.OnNext(ex.Message);
            //        return 0;
            //    }
            //};

            //reg_count = _().GetAwaiter().GetResult();

            //if (reg_count != 0)
            //{
            //    isOpen = true;
            //}
        }


        bool ICAN.isOpen()
        {
            return isOpen;
        }


        void ICAN.Transmit(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }


        public static async Task<uint> ReadRegisterCount(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

            var RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)modbus_id, 26 }); // get device holding count
            
            if (RXbuf[7] != 26)
            {
                Debug.WriteLine(String.Format("Holding count response error FC = {0}", RXbuf[7]));
                return 0;
            }

            int hreg_count = RXbuf[9] + (RXbuf[8] << 8);

            tmp.close();
            return (uint)hreg_count;
        }

        public static async Task<List<byte[]>> ReadRegistersInfoAsync(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            /**
            *   27 function - Get register info

                   ________________________________REQUEST FRAME________________________________
                 *
                 *       +-----+----+-----------------+-----------+
                 * index | 0   | 1  |  2    |  3      |  4  |  5  |
                 *       +-----+----+-----------------+-----------+
                 * FRAME | ADR |CMD | ARD_HI| ARD_LO  |    CRC    |
                 *       +-----+----+-----------------+-----------+
                 *

                    _______________________________RESPONSE FRAME_________________________________
                 *
                 *       +-----+----+--------------+-------+----+-----+----+------------------+---------+
                 * index | 0   | 1  |   2  |   3   |4|5|6|7| 8  |  9  | 10 |                  |    |    |
                 *       +-----+----+--------------+-------+--- +-----+----+------------------+---------+
                 * FRAME | ADR |CMD |adr_hi|adr_lo |MEM_ADR|type|index| RO | info string      |   CRC   |
                 *       +-----+----+--------------+-------+----+-----+----+------------------+---------+
*/
            List<byte[]> deviceParameters = new();


            uint hreg_count = await ReadRegisterCount(server_name, server_port, modbus_id);

            //ushort[] buff = await tmp.ReadHoldingsAsync((byte)modbus_id, 0, hreg_count);

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);
            for (int i = 0; i < hreg_count; i++)
            {
                byte[] RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 4, (byte)modbus_id, 27, 0, (byte)i });
                deviceParameters.Add(RXbuf);
            }

            tmp.close();
            return deviceParameters;
        }

        public static async Task<ushort[]> ReadHRsAsync(uint count, string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

            ushort[] buff = await tmp.ReadHoldingsAsync((byte)modbus_id, 0, (int)count);
            tmp.close();

            return buff;

        }



        void ICAN.CloseConnection()
        {

        }

        void ICAN.Receive()
        {
            //ushort[] buff;
            //if (reg_count == 0) {
            //    return;
            //}

            //try {

            //    buff = ReadHRsAsync(reg_count, server_name, server_port, modbus_id).GetAwaiter().GetResult();

            //}
            //catch (ServerModbusTCPException ex) {
            //    ICAN.LogUpdater.OnNext(ex.Message);
            //    return;
            //}

            //for (int i = 0; i < reg_count; i++)
            //{
            //    byte[] data = new byte[2];

            //    Buffer.BlockCopy(buff, i, data, 0, 2);

            //    ICAN.RxTxUpdater.OnNext(new ICAN.RxTxCanData((uint)i, data));
            //}
            
        }
    }
}
