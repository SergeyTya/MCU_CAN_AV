using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peak.Can.Basic;

namespace CanGPSLogger
{
    internal class CanReader: IEnableLogger
    {
        PcanChannel channel = PcanChannel.Usb01;
        Worker? myWorker;
        public CanReader() {
            myWorker = new Worker(channel, Bitrate.Pcan500);
            myWorker.MessageAvailable += OnMessageAvailable;

            try {
                myWorker.Start();
                this.Log().Info($"PCAN connection started status {0}", 0);
            }
            catch (Peak.Can.Basic.PcanBasicException ex)
            {
                this.Log().Fatal($"PCAN connection  not started ");
                this.Log().Fatal(ex.Message);
            }
        }

        private void OnMessageAvailable(object? sender, MessageAvailableEventArgs e)
        {
           // this.Log().Info($"PCAN get mess");
            PcanMessage message = new PcanMessage(0x100, MessageType.Standard, 3, new byte[] { 1, 2, 3 }, false);
            ulong timestamp = 0;
            ((Worker) sender).Dequeue(out message, out timestamp);
            ((Worker)sender).ClearAllReceiveQueues();


            this.Log().Info($"PCAN get mess ID {message.ID}");
             
        }
    }
}
