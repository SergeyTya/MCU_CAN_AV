﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Can
{
    public interface ICAN
    {
        public static ICAN? CAN;
        public static Subject<string> LogUpdater = new();
        public static Subject<RxTxCanData> RxUpdater =new();
        public static Subject<RxTxCanData> TxUpdater = new();

        public static System.Timers.Timer timer;

        public enum CANType {
            CAN_USBCAN_B = 0,
            ModbusTCP = 1,
            ModbusRTU = 2,
            Dummy = 3 
        }

        public class RxTxCanData
        {
            uint _id;
            public uint id { get { return _id; } }

            byte[] _data;
            public byte[] data { get { return _data; } }

            public RxTxCanData(uint id, byte[] data)
            {
                _id = id;
                _data = data;
            }
        }

        public struct CANInitStruct {

            public UInt32 _devind = 1;
            public UInt32 _canind = 0;
            public UInt32 _Baudrate = 500000;
            public UInt32 _RcvCode = 0;
            public UInt32 _Mask = 0xffffffff;
            public UInt32 _PollInterval_ms = 100;  /// Poll interval, ms

            public string server_name = "localhost";
            public uint   server_port = 8888;

            public string com_name = "COM1";

            public CANInitStruct()
            {
            }
        }

        class ICANException : Exception
        {
            public ICANException(string message):base(message) { }
        }

        abstract void CloseConnection();

        abstract void Receive(); // Must post all recieve message to RxTxUpdater
        abstract void Transmit(RxTxCanData data);
        abstract bool isOpen();

        public static void Close() {


            timer.Stop();
            timer.Dispose();

            LogUpdater.OnNext("Connection closed");

        }

        public static void Create(CANInitStruct InitStructure, CANType CANType)
        {

            switch (CANType)
            {
                case CANType.ModbusTCP:

                    ICAN.CAN = new ModbusTCP.ModbusTCP(InitStructure);
                    break;

                case CANType.CAN_USBCAN_B:
                    ICAN.CAN = new USBCAN_B_win(InitStructure);
                    break;
            }



            if (ICAN.CAN != null)
            {
                LogUpdater.OnNext($"Connection created {ICAN.CAN.GetType().Name}");

                if (ICAN.CAN.isOpen())
                {

                    timer = new System.Timers.Timer(InitStructure._PollInterval_ms);
                    timer.Elapsed += (_, __) =>
                    {
                       // ICAN.CAN.Receive();

                        ICAN.RxUpdater.OnNext(new RxTxCanData(122, new byte[0]));
                    };

                    timer.Start();

                    LogUpdater.OnNext($"Connection open type={CANType}");
                }
                else{
                    LogUpdater.OnNext($"Connection open fail {ICAN.CAN.GetType().Name}");
                }
            }
            else {
                LogUpdater.OnNext($"Connection create fail {ICAN.CAN.GetType().Name}");
            }
        }
    }
}
