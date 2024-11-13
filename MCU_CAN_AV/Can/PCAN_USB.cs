using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Splat;
using Peak.Can.Basic;
using System.Reactive.Linq;
using static MCU_CAN_AV.ViewModels.ConnectionState;

namespace MCU_CAN_AV.Can
{
    internal class PCAN_USB : BaseCAN, IEnableLogger
    {
        Worker myWorker;
        public PCAN_USB(ICAN.CANInitStruct InitStructure) : base(InitStructure)
        {
            PcanChannel _channel = PcanChannel.Usb01;
            Bitrate _bitrate = Bitrate.Pcan500;

            myWorker = new Worker(_channel, _bitrate);
            myWorker.MessageAvailable += OnMessageAvailable;

            try
            {
                myWorker.AllowEchoFrames = false;
                myWorker.Start();
                this.Log().Info($"PCAN connection started at {_channel}:{_bitrate}");

                //Observable.Interval(TimeSpan.FromMilliseconds(700)).Subscribe((_) => {
                //    if (wdg == true)
                //    {
                //        this.Log().Fatal($"CAN timeout");
                //    }
                //    wdg = true;
                //});

            }
            catch (Peak.Can.Basic.PcanBasicException ex)
            {
                this.Log().Fatal($"PCAN connection  not started ");
                this.Log().Fatal(ex.Message);
            }



        }

        bool _isOpen = false;
        public override bool isOpen { get => _isOpen; }

        int transaction_id = 0;
        bool need_post = false;
        public unsafe override void Receive()
        {

            need_post=true;
        }

        public override void Close_instance()
        {
            Api.Uninitialize(PcanChannel.Usb01);
        }

        unsafe public override void Transmit(ICAN.RxTxCanData data)
        {
            PcanMessage message = new PcanMessage(id:data.id, msgType:MessageType.Extended, dlc:8,  data:data.data);
            try {
                var state = myWorker.Transmit(message);
            } catch(Exception e) {
                this.Log().Fatal($"{e.Message}");
            }
        }


        List<PcanMessage> buf = new();
        private void OnMessageAvailable(object? sender, MessageAvailableEventArgs e)
        {
            PcanMessage message = new PcanMessage(0x100, MessageType.Standard, 3, [], false);
            ulong timestamp = 0;
            myWorker.Dequeue(out message, out timestamp);
            myWorker.ClearAllReceiveQueues();
            post(new ICAN.RxTxCanData(message.ID, message.Data, transaction_id: transaction_id++));
        }
    }


}

