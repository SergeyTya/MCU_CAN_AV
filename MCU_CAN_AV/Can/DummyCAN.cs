using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Can
{
    internal class DummyCAN : ICAN
    {
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
              ICAN.RxUpdater.OnNext(new ICAN.RxTxCanData(122, new byte[0]));
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
