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
using System.Data.Common;
using SerialToSocket;

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal class ModbusTCP : BaseCAN, IEnableLogger
    {
        uint reg_count = 0;
        string server_name = "localhost";
        uint server_port = 8888;
        uint modbus_id = 1;

        bool _isOpen = false;
        bool _connected = false;

        static SemaphoreSlim semaphoreSlim  = new SemaphoreSlim(1);
        static SemaphoreSlim semaphoreSlim2 = new SemaphoreSlim(1);
        ServerModbusTCP RxConnection;
        ServerModbusTCP TxConnection;

        ISerialToSocket server = new SerialToSocket.SerialToSocket();

        public ModbusTCP(ICAN.CANInitStruct InitStruct): base(InitStruct)
        {
            try{

                server.Start(InitStructure.com_name, (int) InitStruct._Baudrate, InitStruct.server_name, (int) InitStruct.server_port);

            }
            catch(Exception e)
            {
                this.Log().Error(e.Message);
            }

            modbus_id   = InitStructure._devind;
            server_port = InitStructure.server_port;
            server_name = InitStructure.server_name;

            Task.Run(async () => {

                RxConnection = new ServerModbusTCP(server_name, (int)server_port);
                TxConnection = new ServerModbusTCP(server_name, (int)server_port);

                await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
                try {
                    var res = await ReadRegisterCount(server_name, server_port, modbus_id).ConfigureAwait(false);
                    return res;
                } finally { semaphoreSlim.Release(); }

            }).ToObservable().Take(1).Subscribe(
                (_) => { reg_count = _; this.Log().Info($"Found  {_} registers"); _connected = true; },
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

                await semaphoreSlim2.WaitAsync();
                try {
                    ushort[] tmp_us = new ushort[] { BitConverter.ToUInt16(data.data.ToArray(), 0) };
                    await TxConnection.WriteHoldingsAsync((int)modbus_id, (byte)data.id, tmp_us);
                }finally { 
                    semaphoreSlim2.Release(); 
                }

            });
        }


        public async Task<uint> ReadRegisterCount(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {
            if (RxConnection == null) throw new Exception("Connection error");
            var RXbuf = await RxConnection.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)modbus_id, 26 }); // get device holding count
            
            if (RXbuf[7] != 26)
            {
                throw new Exception("Reading registers count - Timeout ");
            }

            int hreg_count = RXbuf[9] + (RXbuf[8] << 8);

            return (uint)hreg_count;
        }

        //public override byte[][] ReadDeviceDescriptionsBytes() { 
            //TODO
        //}

        public async Task<List<byte[]>> ReadRegistersInfoAsync(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
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

              

                for (int i = 0; i < hreg_count; i++)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    byte[] RXbuf = await RxConnection.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 4, (byte)modbus_id, 27, 0, (byte)i });
                    deviceParameters.Add(RXbuf);
                    stopwatch.Stop();
                    //this.Log().Info($"   Register {i} info readed  - {stopwatch.ElapsedMilliseconds} ms");
                }
               

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

         public async Task<string> getDevId(string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
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

            if (RxConnection == null) throw new Exception("Connection error");
            var RXbuf = await RxConnection.SendRawDataAsync(new byte[] { 0, 0, 0, 0, 0, 2, (byte)modbus_id, 0x2B }); // get device holding count
            string res = Encoding.UTF8.GetString(RXbuf.ToList().GetRange(7, RXbuf.Length - (4 + 7)).ToArray());
            return res;
         }

        public async Task<ushort[]> ReadHRsAsync(uint count, string server_name = "localhost", uint server_port = 8888, uint modbus_id = 1)
        {

            ushort[] buff = await RxConnection.ReadHoldingsAsync((byte)modbus_id, 0, (int)count);

            return buff;

        }

        public override void Close_instance()
        {
            server?.Stop();
            _isOpen = false;
            RxConnection?.close();
            Dispose();
        }

        public override void Receive()
        {
            if (!_connected) return;
            if (!_isOpen) return;
            if (semaphoreSlim.CurrentCount == 0) return;
            if (RxConnection == null) return;

                Task.Run(async () =>
                {
                //if (RxConnection == null) {
                //    await Task.Delay(10000);
                //    throw new Exception("No server avalible");
                //};

                await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
                try {
                    var res = await ReadHRsAsync(reg_count, server_name, server_port, modbus_id).ConfigureAwait(false);
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

                        post(new ICAN.RxTxCanData((uint)i, data));
                    }
                },
                exception => { 
                   this.Log().Error(exception.Message);
                    post(new ICAN.RxTxCanData { Timeout = true });
                }
            );

        }
    }
}
