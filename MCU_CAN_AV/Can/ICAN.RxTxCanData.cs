namespace MCU_CAN_AV.Can
{
    public partial interface ICAN
    {
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
                _data = new byte[] {0};
            }

        }
    }
}
