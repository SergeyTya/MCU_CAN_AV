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

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal class ModbusTCP : ICAN
    {

        ConnectionSetups connection_setups = new();

        public ModbusTCP(ICAN.CANInitStruct init)
        {
            connection_setups.SlaveAdr = (int)init._devind;
        }

 
        bool ICAN.isOpen()
        {
            throw new NotImplementedException();
        }

  
        void ICAN.Transmit(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }


        public async Task<List<byte[]>?> ReadRegistersInfoAsync()
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
            connection_setups = ConnectionSetups.read();
            List<byte[]> deviceParameters = new();

            try
            {

                ServerModbusTCP tmp = new ServerModbusTCP(connection_setups.ServerName, connection_setups.ServerPort);

                var RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)connection_setups.SlaveAdr, 26 }); // get device holding count
                if (RXbuf[7] != 26)
                {
                    Debug.WriteLine(String.Format("Holding count response error FC = {0}", RXbuf[7]));
                    return null;
                }

                int hreg_count = RXbuf[9] + (RXbuf[8] << 8);

                ushort[] buff = await tmp.ReadHoldingsAsync((byte)connection_setups.SlaveAdr, 0, hreg_count);


                for (int i = 0; i < hreg_count; i++)
                {
                    RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 4, (byte)connection_setups.SlaveAdr, 27, 0, (byte)i });
                    deviceParameters.Add(RXbuf);
                }
            }
            catch (ServerModbusTCPException ex)
            {

                Debug.WriteLine(ex.Message);
                // log_data(ex.Message);
                return null;

            }

            return deviceParameters;
        }

        void ICAN.CloseConnection()
        {
            throw new NotImplementedException();
        }

        void ICAN.Receive()
        {
            throw new NotImplementedException();
        }
    }
}
