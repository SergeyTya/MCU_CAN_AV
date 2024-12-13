using AsyncSocketTest;
using MCU_CAN_AV.Devices;
using Microsoft.AspNetCore.SignalR;
using ScottPlot;
using Splat;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static AsyncSocketTest.ServerModbusTCP;

namespace MCU_CAN_AV.Can.ModbusTCP
{
    internal class SerialRTU : BaseCAN, IEnableLogger, IModbusTransport
    {
        private SerialPort port =new();
        static readonly Stopwatch timer_wdg = new Stopwatch();
        private IObservable<long> wdg = Observable.Interval(TimeSpan.FromSeconds(0.5));
        public bool receiveTimeout = false;
        ICAN.CANInitStruct com_setup;
        private List<byte> Rxbuf = new List<byte>();

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        static Subject<byte[]?> RxSub = new Subject<byte[]?>();

        bool _isOpen = false;
        bool _connected = false;
        uint reg_count = 0;
        bool _init_done = false;


        public SerialRTU(ICAN.CANInitStruct InitStructure) : base(InitStructure)
        {

            com_setup = InitStructure;

            port.DataReceived += Port_DataReceived;
            port.ReadBufferSize = 4096;

            Connect();

            wdg.Subscribe((_) =>
            {
                if (timer_wdg.IsRunning)
                {
                    if (timer_wdg.ElapsedMilliseconds > 500)
                    {
                        this.Log().Fatal($"Serial timeout");
                        timer_wdg.Reset();
                        Rxbuf.Clear();
                        receiveTimeout = true;
                        RxSub.OnNext(null);
                    }
                }
            });


            Task.Run(async () => {

                var res = await ReadRegisterCount().ConfigureAwait(false);
                return res;

            }).ToObservable().Take(1).Subscribe(
                (_) => { reg_count = _; this.Log().Info($"Found  {_} registers"); _connected = true; },
                exeption => {
                    this.Log().Error(exeption.Message);
                }
            );
            _isOpen = true;
        }

        public void Connect()
        {
            this.Log().Info($"Connecting {InitStructure.com_name} : {InitStructure._Baudrate} : {InitStructure._devind} ");

            if (port == null) {
                return;
            }

            if (port.IsOpen) port.Close();
            port.PortName = InitStructure.com_name;
            port.BaudRate = (int) InitStructure._Baudrate;
            receiveTimeout = false;
            port.WriteTimeout = 100;
            try
            {
                port.Open();
            }
            catch (Exception e)
            {
                this.Log().Fatal(e);
            }
        }

        public override void Close_instance()
        {
           port.Close();
        }

        public override bool isOpen
        {
            get => port.IsOpen;
        }


        public override void Receive()
        {
            if (!_connected) return;
            if (!_isOpen) return;
            if (semaphoreSlim.CurrentCount == 0) return;
            if (!_init_done) return;
        
            Task.Run(async () =>
            {
                var res = await ReadHRsAsync(reg_count).ConfigureAwait(false);
                return res;

            }).ToObservable().Take(1).Subscribe(
            (_) =>
            {
                for (int i = 0; i < reg_count; i++)
                {
                    byte[] data = new byte[2];

                    Buffer.BlockCopy(_, i * 2, data, 0, 2);
                    post(new ICAN.RxTxCanData((uint)i, data, transaction_id: Task.CurrentId));

                }
            },
            exception => {
                this.Log().Error(exception.Message);
                post(new ICAN.RxTxCanData { Timeout = true });
            }
        );

        }

        public override void Transmit(ICAN.RxTxCanData data)
        {
            Task.Run(async () => {

                await semaphoreSlim.WaitAsync();
                try
                {
                    ushort[] tmp_us = new ushort[] { BitConverter.ToUInt16(data.data.ToArray(), 0) };
                    //await TxConnection.WriteHoldingsAsync((int)com_setup._devind, (byte)data.id, tmp_us);
                    List<byte> req = new List<byte>() {
                        (byte)com_setup._devind,
                        0x10,
                        (byte) (data.id>>8),
                        (byte) (data.id&0xff),
                        (byte)((tmp_us.Length) >> 8),
                        (byte)((tmp_us.Length) & 0xff),
                        (byte)(tmp_us.Length*2)
                    };
                    foreach (var item in tmp_us)
                    {  // change indian
                        var res = BitConverter.GetBytes(item);
                        Array.Reverse(res);
                        req.AddRange(res);
                    }

                    serial_write(req);
                    var response = await RxSub.FirstAsync().ToTask();

                    if ( response != null ) {
                        if (response.Count() < 4) 
                        {
                            if (response[1] == 0x10)
                            {
                                return;
                            }
                        }
                    }

                    throw new ServerModbusTCPException("Not valid response: WriteHoldingsAsync");
                }
                finally
                {
                    semaphoreSlim.Release();
                }

            });
        }
      
        public async Task<string> getDevId()
        {
            await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
            try
            {
                string devId = String.Empty;
                List<byte> req = new List<byte>() { (byte)com_setup._devind, 0x2B, 0xE, 0x1, 0x1 };
                serial_write(req);

                var res = await RxSub.FirstAsync().ToTask();

                if (res != null)    
                {
                    if (res.Length > 4) {
                      //  if (res[1] == 0x2B) { // FIX this at MCU side
                            devId = Encoding.UTF8.GetString(res.ToList().GetRange(2, res.Length - 4).ToArray());
                            _init_done = true;
                            return devId;
                      //  }
                    }
                }

                

                throw new ServerModbusTCPException("Not valid response: getDevId");
            }
            finally { 
                semaphoreSlim.Release();
            }


         
        }

        public async Task<ushort[]> ReadHRsAsync(uint count)
        {
            await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
            try
            {
                List<byte> req = new List<byte>() { (byte)com_setup._devind, 3, 0, 0, (byte)(count >> 8), (byte)(count & 0xff) };
                serial_write(req);

                var res = await RxSub.FirstAsync().ToTask();


                if (res != null)
                {
                    if (res.Length > 4)
                    {
                        if (res[1] == 3)
                        {
                            var data = res.ToList().GetRange(3, res[2]).ToArray();
                            var retval = ConvertFromByte(data);
                            return retval;
                        }
                    }
                }

                throw new ServerModbusTCPException("Not valid response: ReadHRsAsync");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<List<byte[]>> ReadRegistersInfoAsync()
        {
            
            List<byte[]> deviceParameters = new();

            uint hreg_count = await ReadRegisterCount();

            await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
            try
            {

                for (int i = 0; i < hreg_count; i++)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    List<byte> req = new List<byte>() { (byte)com_setup._devind, 27, 0, (byte)i };
                    serial_write(req);

                    var res = await RxSub.FirstAsync().ToTask();

                    if (res == null) {
                        throw new ServerModbusTCPException("Not valid response: ReadRegistersInfoAsync");
                    }
                    // RTU frame to TCP frame
                    List <byte> RXbuf = new();
                    RXbuf.AddRange(new byte[] { 0, 0, 0, 0, 0, (byte) (res.Length-2)});
                    RXbuf.AddRange(res.ToList().GetRange(0, res.Length - 2).ToArray());

                    deviceParameters.Add(RXbuf.ToArray());
                    stopwatch.Stop();
                   // this.Log().Info($"   Register {i} info readed  - {stopwatch.ElapsedMilliseconds} ms");
                }


                return deviceParameters;
            }
            finally
            {
                semaphoreSlim.Release();
            }

        }

        public async Task<uint> ReadRegisterCount()
        {
            await semaphoreSlim.WaitAsync(/*TimeSpan.FromSeconds(0.3)*/);
            try
            {
                int ret_val = 0;

                List<byte> req = new List<byte>() { (byte)com_setup._devind, 26 };
                serial_write(req);

                var res = await RxSub.FirstAsync().ToTask();

                if (res != null)
                {
                    if (res.Length == 6) {
                        if (res[1] == 26) {
                            ret_val = res[3] + (res[2] << 8);
                        }
                    }
                }

                return (uint) ret_val;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private void serial_write(List<byte> req)
        {
            if (port == null) return;

            UInt16 crc = chMBCRC16(req.ToArray(), (ushort)req.Count);

            req.Add((byte)(0x00FF & crc));
            req.Add((byte)((0xFF00 & crc) >> 8));
            if (timer_wdg.IsRunning)
            {
                this.Log().Error($"Skip write request. Port is on wait");
                return;
            }
            port.ReadExisting();
            port.Write(req.ToArray(), 0, req.Count);
            timer_wdg.Restart();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            UInt16 crc = 0;
            UInt16 frame_crc = 1;

            var size = port.BytesToRead;
            byte[] data = new byte[size];
            port.Read(data, 0, size);
            Rxbuf.AddRange(data);

            size = Rxbuf.Count;
            if(size > 4) {
                crc = chMBCRC16(Rxbuf.ToArray(), (ushort)(size - 2));
                frame_crc = (UInt16)(Rxbuf.ToArray()[size - 2] + (Rxbuf.ToArray()[size - 1] << 8));
            }

            if (crc == frame_crc)
            {
                timer_wdg.Stop();
               

                switch (Rxbuf[1])
                {
                    case 0x2b: // Report device info
                    case 0x10: // Read write holdings
                    case 27:   // ReadRegistersInfoAsync
                    case 26:   // ReadRegisterCount
                    case 3:    // ReadHoldings
                    case 7:    // writeHoldings
                    
                        RxSub.OnNext(Rxbuf.ToArray());

                        break;

                    default:
                        this.Log().Info($"Unknow responce 0x{Rxbuf[1].ToString("X2")}");
                        RxSub.OnNext(Rxbuf.ToArray());
                        break;
                }

                Rxbuf.Clear();
            }
        }

        static byte[] aucCRCLo = {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
        0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
        0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
        0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
        0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
        0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
        0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
        0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
        0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
        0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
        0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
        0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
        0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
        0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
        0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
        0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
        0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
        0x41, 0x81, 0x80, 0x40};
        static byte[] aucCRCHi = {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40
        };

        static public UInt16 chMBCRC16(byte[] pucFrame, UInt16 len)
        {
            byte ucCRCHi = 0xFF;
            byte ucCRCLo = 0xFF;
            int iIndex;

            for (int i = 0; i < len; i++)
            {
                try
                {
                    iIndex = ucCRCLo ^ pucFrame[i];
                    ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[iIndex]);
                    ucCRCHi = aucCRCLo[iIndex];
                }
                catch (Exception er) { return 0; };
            }

            return (UInt16)(ucCRCLo + (ucCRCHi << 8));
        }

        public UInt16[] ConvertFromByte(byte[] data)
        {
            int index = 0;
            var res = data.GroupBy(x => (index++) / 2).Select(x => BitConverter.ToUInt16(x.Reverse().ToArray(), 0)).ToList();
            return res.ToArray();
        }

    }
}
