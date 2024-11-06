using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Splat;
using Peak.Can.Basic;

namespace MCU_CAN_AV.Can
{
    internal class PCAN_USB : BaseCAN, IEnableLogger
    {
        public PCAN_USB(ICAN.CANInitStruct InitStructure) : base(InitStructure)
        {
            PcanStatus result;
            result = Api.Initialize(PcanChannel.Usb01, Bitrate.Pcan500);

            var status = Api.GetStatus(PcanChannel.Usb01);
           
            if (status != PcanStatus.OK)
            {
                this.Log().Fatal($"PCAN connection status {0}", "ok");
                _isOpen = true;
            }
            else 
            {
                this.Log().Info($"PCAN connection status {0}", "fail");
            }


        }

        bool _isOpen = false;
        public override bool isOpen { get => _isOpen; }

        int transaction_id = 0;
        public unsafe override void Receive()
        {

            PcanMessage message = new PcanMessage(0, MessageType.Extended, 3, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8}, false);
            Api.Read(PcanChannel.Usb01, out message);

            transaction_id++;
            post(new ICAN.RxTxCanData(message.ID, message.Data, transaction_id: transaction_id));
        }

        public override void Close_instance()
        {
            Api.Uninitialize(PcanChannel.Usb01);
        }

        unsafe public override void Transmit(ICAN.RxTxCanData data)
        {
            PcanMessage message = new PcanMessage(0, MessageType.Extended, 3, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, false);
            message.ID = data.id;
            message.Data = data.data;
            Api.Write(PcanChannel.Usb01, message);
        }

    }
}
