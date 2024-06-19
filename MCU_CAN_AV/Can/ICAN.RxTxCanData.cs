namespace MCU_CAN_AV.Can
{
    public partial interface ICAN
    {
        public class RxTxCanData
        {
            bool _timeout;
            public bool Timeout { get { return _timeout; } set { _timeout = value; } }

            bool _needUpdate = false;
            public bool NeedUpdate { get { return _needUpdate; } set { _needUpdate = value; } }

            uint _id;
            public uint id { get { return _id; } }

            byte[] _data;
            public byte[] data { get { return _data; } }

            int _transaction_id;
            public int transaction_id { get { return _transaction_id; } }

            public RxTxCanData(uint id, byte[] data, int? transaction_id=0)
            {
                _id = id;
                _data = data;
                if (transaction_id != null)
                {
                    _transaction_id = (int)transaction_id;
                }
                else {
                    transaction_id = -1;
                }
                
            }

            public RxTxCanData()
            {
                _id = 0;
                _data = new byte[] {0};
            }

        }
    }
}
