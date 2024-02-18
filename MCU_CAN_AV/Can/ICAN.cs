using MCU_CAN_AV.Devices.Dummy;
using System;
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
        class ICANException : Exception
        {
            public ICANException(string message) : base(message) { }
        }
        public enum CANType
        {
            CAN_USBCAN_B = 0,
            ModbusTCP = 1,
            ModbusRTU = 2,
            Dummy = 3
        }

        public class RxTxCanData
        {
            bool _timeout;
            public bool Timeout { get { return _timeout; } set { _timeout = value; } }

            uint _id;
            public uint id { get { return _id; } }

            byte[] _data;
            public byte[] data { get { return _data; } }

            public RxTxCanData(uint id, byte[] data)
            {
                _id = id;
                _data = data;
            }

            public RxTxCanData()
            {
                _id = 0;
                _data = null;
            }

        }

        public struct CANInitStruct
        {

            public UInt32 _devind = 1;
            public UInt32 _canind = 0;
            public UInt32 _Baudrate = 500000;
            public UInt32 _RcvCode = 0;
            public UInt32 _Mask = 0xffffffff;
            public UInt32 _PollInterval_ms = 100;  /// Poll interval, ms

            public string server_name = "localhost";
            public uint server_port = 8888;

            public string com_name = "COM1";

            public CANInitStruct()
            {
            }
        }


        public static ICAN? CAN;
        public static Subject<string> LogUpdater = new();
        public static Subject<RxTxCanData> RxUpdater =new();
        public static Subject<RxTxCanData> TxUpdater = new();

        public CANInitStruct InitStructure { get; }

        public static System.Timers.Timer timer;

        abstract void Close();
        abstract void Receive(); // Must post all recieve message to RxTxUpdater
        abstract void Transmit(RxTxCanData data);
        abstract bool isOpen();

        public static void Dispose() {

            timer.Stop();
            timer.Dispose();
            timer = null;
            ICAN.CAN.Close();
            ICAN.CAN = null;
            LogUpdater.OnNext("ICAN Connection closed");
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

                case CANType.Dummy:
                    ICAN.CAN = new DummyCAN(InitStructure);
                    break;
            }


            if (ICAN.CAN != null)
            {
                if (ICAN.CAN.isOpen())
                {

                    timer = new System.Timers.Timer(InitStructure._PollInterval_ms);
                    timer.Elapsed += (_, __) =>
                    {
                        ICAN.CAN.Receive();
                    };

                    timer.Start();

                    LogUpdater.OnNext($" !!!! {CANType} connection opened");
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
