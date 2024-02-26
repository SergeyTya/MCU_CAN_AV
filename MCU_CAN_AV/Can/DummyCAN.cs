using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public override void Receive()
        {
            if (id++ > 5) id = 0;   
            if (!faultmode)
            {
                Random rnd = new Random();
                byte[] data = { 0, 0, 0, 0, 0, 0 };
                rnd.NextBytes(data);
                post(new RxTxCanData(id, data));
                return;
            }
            else {
                post (new ICAN.RxTxCanData { Timeout = true } );
                return;
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
