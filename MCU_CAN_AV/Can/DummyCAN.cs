using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MCU_CAN_AV.Can.ICAN;

namespace MCU_CAN_AV.Can
{
    internal class DummyCAN : ICAN
    {
        public bool faultmode = false;
        CANInitStruct _initStructure;

        public DummyCAN(CANInitStruct InitStructure) {
            this._initStructure = InitStructure;
        }  

        public ICAN.CANInitStruct InitStructure => _initStructure;

        public void CloseConnection()
        {
            throw new NotImplementedException();
        }

        public bool isOpen()
        {
            return true;
        }

        public void Receive()
        {
            if (!faultmode)
            {
                ICAN.RxUpdater.OnNext(new ICAN.RxTxCanData(2, new byte[1] { 0 }));
            }
            else {
                ICAN.RxUpdater.OnNext((new ICAN.RxTxCanData{Timeout = true}));
            }
        }

        public void Transmit(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            
        }
    }
}
