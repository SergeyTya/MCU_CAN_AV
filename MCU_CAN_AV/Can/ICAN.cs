using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Can
{
    public interface ICAN
    {
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
            public UInt32 _devind = 0;
            public UInt32 _canind = 0;
            public UInt32 _Baudrate = 0;
            public UInt32 _RcvCode = 0;
            public UInt32 _Mask = 0xffffffff;
            public UInt32 _PollInterval_ms = 100;  /// Poll interval, ms

            public CANInitStruct(
                UInt32 DevId = 0,
                UInt32 CANId = 0,
                UInt32 Baudrate = 500,
                UInt32 RcvCode = 0,
                UInt32 Mask = 0xffffffff,
                UInt32 Interval = 100   /// Poll interval, ms
           )
            {
                _devind = DevId;
                _canind = CANId;
                _Baudrate = Baudrate;
                _Mask = Mask;
                _RcvCode= RcvCode;
                _PollInterval_ms = Interval;
            }
        }

        class ICANException : Exception
        {
            public ICANException(string message):base(message) { }
        }

        public abstract void InitCAN(CANInitStruct init);
        public abstract IObservable<RxTxCanData> Start();
        public abstract void Close();
        public abstract void Transmit(RxTxCanData data);
        public abstract bool isOpen();

        public static ICAN Create(CANInitStruct init) { 
            return new USBCAN_B_win(init);
        }
    }
}
