using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MCU_CAN_AV.Can.ICAN;

namespace MCU_CAN_AV.Can
{
    internal class BaseCAN : ICAN, IDisposable, IEnableLogger
    {
        internal Subject<RxTxCanData> RxUpdater = new();

        System.Timers.Timer? timer ;

        ICAN.CANInitStruct _InitStructure;

        public BaseCAN(ICAN.CANInitStruct InitStructure) {

            _InitStructure = InitStructure;

            timer = new System.Timers.Timer(_InitStructure._PollInterval_ms);
            timer.Elapsed += (_, __) =>
            {
                var mes = Receive();

                if(mes == null) { return; }

                foreach (var item in mes)
                {
                    RxUpdater.OnNext(item);
                    if (item.Timeout)
                    {
                        timer.Interval = 1000;
                    }
                    else
                    {
                        timer.Interval = _InitStructure._PollInterval_ms;
                    }
                }
            };

            timer?.Start();
        }

        public void Close()
        {
            Close_instance();
            Dispose();
        }

        public void Dispose()
        {
            RxUpdater.Dispose();
            timer?.Stop();
            timer?.Dispose();
        }

        public IObservable<ICAN.RxTxCanData> RxObservable => RxUpdater;


        public CANInitStruct InitStructure => _InitStructure;

        /// <summary>
        ///  Close method for children
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Close_instance()
        {
            throw new NotImplementedException();
        }

        public virtual bool isOpen { get => 
            
            throw new NotImplementedException(); 
        }
        

        public virtual RxTxCanData[]? Receive()
        {
            throw new NotImplementedException();
        }

        public virtual void Transmit(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }

        public virtual byte[][] ReadDeviceDescriptionsBytes()
        {
            throw new NotImplementedException(); //TODO
        }

        public virtual byte[] ReadDeviceInfoBytes()
        {
            throw new NotImplementedException(); //TODO
        }
    }
}
