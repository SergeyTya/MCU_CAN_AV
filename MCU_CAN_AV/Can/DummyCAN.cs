using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MCU_CAN_AV.Can.ICAN;

namespace MCU_CAN_AV.Can
{
    internal class DummyCAN : BaseCAN
    {
        public bool faultmode = false;
      
        public DummyCAN(CANInitStruct InitStructure) : base(InitStructure) { }

        public void CloseConnection()
        {
            throw new NotImplementedException();
        }

        public override bool isOpen { get => true; }
        

        public override RxTxCanData[]? Receive()
        {
            if (!faultmode)
            {
                return new RxTxCanData[] { new RxTxCanData(2, new byte[1] { 0 }) };
            }
            else {
                return new RxTxCanData[] { new ICAN.RxTxCanData { Timeout = true } };
            }
        }

        public void Transmit(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }

        public override void Close_instance()
        {
            
        }
    }
}
