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

        uint id = 0;
        public override RxTxCanData[]? Receive()
        {
            if (id++ > 5) id = 0;   
            if (!faultmode)
            {
                Random rnd = new Random();
                byte[] data = { 0, 0, 0, 0, 0, 0 };
                rnd.NextBytes(data);
                return new RxTxCanData[] { new RxTxCanData(id, data)};
            }
            else {
                return new RxTxCanData[] { new ICAN.RxTxCanData { Timeout = true } };
            }
        }

        public override void Transmit(ICAN.RxTxCanData data)
        {
            
        }

        public override void Close_instance()
        {
            
        }
    }
}
