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
using System.Reactive.Threading.Tasks;
using System.Threading;

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal class ModbusTCP : ICAN
    {
        uint reg_count = 0;
        string server_name = "localhost";
        uint server_port = 8888;
        uint modbus_id = 1;

        bool isOpen = false;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        ICAN.CANInitStruct InitStruct;

        public ICAN.CANInitStruct InitStructure => InitStruct;



        public ModbusTCP(ICAN.CANInitStruct InitStruct)
        {
          
            this.InitStruct = InitStruct;
            modbus_id = InitStruct._devind;
            server_port = InitStruct.server_port;
            server_name = InitStruct.server_name;

            Task.Run(async () => {

                await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
                try {
                    var res = await ModbusTCP.ReadRegisterCount(server_name, server_port, modbus_id).ConfigureAwait(false);
                    return res;
                } finally { semaphoreSlim.Release(); }

            }).ToObservable().Take(1).Subscribe(
                (_) => { reg_count = _; ICAN.LogUpdater.OnNext($"Found  {_} registers");},
                exeption => { 
                    ICAN.LogUpdater.OnNext(exeption.Message);
                }
            );
            isOpen = true;
          
        }


        bool ICAN.isOpen()
        {
            return isOpen;
        }


        void ICAN.Transmit(ICAN.RxTxCanData data)
        {
            Task.Run(async () => {
                ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);
                tmp.Timeout = 1000;
                await tmp.WriteHoldingsAsync((int)modbus_id, (byte)data.id, new ushort[]{BitConverter.ToUInt16(data.data.ToArray(), 0) });
            });
        }


        public static async Task<uint> ReadRegisterCount(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

            var RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)modbus_id, 26 }); // get device holding count
            
            if (RXbuf[7] != 26)
            {
                var str = String.Format("Holding count response error FC = {0}", RXbuf[7]);
                ICAN.LogUpdater.OnNext(str);
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

            await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
            try
            {

                uint hreg_count = await ReadRegisterCount(server_name, server_port, modbus_id);

                //ushort[] buff = await tmp.ReadHoldingsAsync((byte)modbus_id, 0, hreg_count);

                ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

                for (int i = 0; i < hreg_count; i++)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    byte[] RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 4, (byte)modbus_id, 27, 0, (byte)i });
                    deviceParameters.Add(RXbuf);
                    stopwatch.Stop();
                    ICAN.LogUpdater.OnNext($"   Register {i} info readed  - {stopwatch.ElapsedMilliseconds} ms");
                }
                tmp.close();
                return deviceParameters;
            }
            finally
            {
                semaphoreSlim.Release();
            }

        }

        public static async Task<ushort[]> ReadHRsAsync(uint count, string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

            ushort[] buff = await tmp.ReadHoldingsAsync((byte)modbus_id, 0, (int)count);
            tmp.close();

            return buff;

        }

        void ICAN.Close()
        {
            isOpen = false;
            Debug.WriteLine(semaphoreSlim.CurrentCount);
        }

        void ICAN.Receive()
        {
            if (!isOpen) return;
            if (semaphoreSlim.CurrentCount == 0) return;
            Task.Run(async () =>
            {
                await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
                try {
                    var res = await ReadHRsAsync(reg_count, server_name, server_port, modbus_id).ConfigureAwait(false);
                   // await Task.Delay(1000);
                    return res;
                }
                finally { 
                    semaphoreSlim.Release(); 
                }
                
            }).ToObservable().Take(1).Subscribe(
                (_) =>
                {
                    for (int i = 0; i < reg_count; i++)
                    {
                        byte[] data = new byte[2];


                        Buffer.BlockCopy(_, i*2, data, 0, 2);

                        ICAN.RxUpdater.OnNext(new ICAN.RxTxCanData((uint)i, data));
                    }
                },
                exeption => { 
                    ICAN.LogUpdater.OnNext(exeption.Message);
                    ICAN.RxUpdater.OnNext(new ICAN.RxTxCanData() { Timeout = true } );
                }
            );
        }
    }
}
