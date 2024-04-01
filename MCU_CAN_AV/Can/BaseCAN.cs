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

        IDisposable? disp;

        System.Timers.Timer? timer ;

        ICAN.CANInitStruct _InitStructure;

        public BaseCAN(ICAN.CANInitStruct InitStructure) {

            _InitStructure = InitStructure;

            // Start hardware polling timer  
            timer = new System.Timers.Timer(_InitStructure._PollInterval_ms);
            timer.Elapsed += (_, __) =>
            {
                // polling hardware
                Receive();


            };

            timer?.Start();

            disp = RxUpdater.Subscribe((_)=>{

                if (timer == null) return;
                if (_.Timeout)
                {
                    timer.Interval = 1000;
                }
                else
                {
                    timer.Interval = _InitStructure._PollInterval_ms;
                }
            });
        }

        internal void post(ICAN.RxTxCanData data) {
            
            if(!RxUpdater.IsDisposed) RxUpdater.OnNext( data );
        }

        public void Close()
        {
            Close_instance();
            Dispose();
        }

        public void Dispose()
        {
            disp?.Dispose();
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
        

        public virtual void Receive()
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
