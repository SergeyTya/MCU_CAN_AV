using AsyncSocketTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using Splat;
using static MCU_CAN_AV.Can.ICAN;

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal class ModbusTCP : BaseCAN, IEnableLogger
    {
        uint reg_count = 0;
        string server_name = "localhost";
        uint server_port = 8888;
        uint modbus_id = 1;

        bool _isOpen = false;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

     
        public ModbusTCP(ICAN.CANInitStruct InitStruct): base(InitStruct)
        {
          
            modbus_id   = InitStructure._devind;
            server_port = InitStructure.server_port;
            server_name = InitStructure.server_name;

            Task.Run(async () => {

                await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
                try {
                    var res = await ModbusTCP.ReadRegisterCount(server_name, server_port, modbus_id).ConfigureAwait(false);
                    return res;
                } finally { semaphoreSlim.Release(); }

            }).ToObservable().Take(1).Subscribe(
                (_) => { reg_count = _; this.Log().Info($"Found  {_} registers");},
                exeption => {
                    this.Log().Error(exeption.Message);
                }
            );
            _isOpen = true;
          
        }


        public override bool isOpen { get => _isOpen; }



        public override void Transmit(ICAN.RxTxCanData data)
        {
            Task.Run(async () => {
                ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);
                //     tmp.Timeout = 1000;
                ushort[] tmp_us = new ushort[] { BitConverter.ToUInt16(data.data.ToArray(), 0) };
                await tmp.WriteHoldingsAsync((int)modbus_id, (byte)data.id, tmp_us);
            });
        }


        public static async Task<uint> ReadRegisterCount(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

            var RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)modbus_id, 26 }); // get device holding count
            
            if (RXbuf[7] != 26)
            {
                throw new Exception("Reading registers count - Timeout ");
            }

            int hreg_count = RXbuf[9] + (RXbuf[8] << 8);

            tmp.close();
            return (uint)hreg_count;
        }

        //public override byte[][] ReadDeviceDescriptionsBytes() { 
            //TODO
        //}

        public static async Task<List<byte[]>> ReadRegistersInfoAsync(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {
            await Task.Delay(300);
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
                    //this.Log().Info($"   Register {i} info readed  - {stopwatch.ElapsedMilliseconds} ms");
                }
                tmp.close();

                return deviceParameters;
            }
            finally
            {
                semaphoreSlim.Release();
            }

        }

        //public override byte[] ReadDeviceInfoBytes() { 
        
            //TODO
        //}

         public static async Task<string> getDevId(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
         {
            /**
            *   0x2B function - Report device info

            *        _________________________RESPONSE FRAME________________________________
            *
            *       +-----+----+-----------------+---------+
            * index | 0   | 1  |                 |    |    |
            *       +-----+----+-----------------+---------+
             * FRAME | ADR |CMD |   info string   |   CRC   |
            *       +-----+----+-----------------+---------+

            */
            
            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int) server_port);
            var RXbuf = await tmp.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)modbus_id, 0x2B }); // get device holding count
            string res = Encoding.UTF8.GetString(RXbuf.ToList().GetRange(7, RXbuf.Length - (4 + 7)).ToArray());
            return res;
         }

        public static async Task<ushort[]> ReadHRsAsync(uint count, string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ServerModbusTCP tmp = new ServerModbusTCP(server_name, (int)server_port);

            ushort[] buff = await tmp.ReadHoldingsAsync((byte)modbus_id, 0, (int)count);
            tmp.close();

            return buff;

        }

        public override void Close_instance()
        {
            _isOpen = false;
            Dispose();
        }

        public override RxTxCanData[]? Receive()
        {
            if (!_isOpen) return null;
            if (semaphoreSlim.CurrentCount == 0) return null;

            ICAN.RxTxCanData[] ret_val = new RxTxCanData[] { new() { Timeout = true } };
        
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

                       ret_val = new RxTxCanData[] { new ICAN.RxTxCanData((uint)i, data) };
                    }
                },
                exception => { 
                   this.Log().Error(exception.Message);
                }
            );

            return ret_val;
        }
    }
}
