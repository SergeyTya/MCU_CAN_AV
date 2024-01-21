using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using MCU_CAN_AV.Can;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace MCU_CAN_AV.Models
{
    internal class tester: IModel
    {
        public IObservable<ICAN.RxTxCanData> updater;

        ICAN.RxTxCanData[] testing_data = { 
            new ICAN.RxTxCanData(1, new byte[] {0,0,0,0 }),
            new ICAN.RxTxCanData(2, new byte[] {0,0,0,0 }),
            new ICAN.RxTxCanData(3, new byte[] {0,0,0,0 })
        };

        public tester() {

           updater = Observable.Create<ICAN.RxTxCanData>(
           observer =>
           {

               System.Timers.Timer timer = new System.Timers.Timer(100);
               timer.Elapsed += (_, __) =>
               {
                   Receive(observer);
               };

               timer.Start();
               return Disposable.Create(() =>
               {
                   timer.Stop();
                   timer.Dispose();
                   Debug.WriteLine("Rx observer Dispose");
               });
           });
        }

        int cnt = 0;
        void Receive(IObserver<ICAN.RxTxCanData> observer) {

            if (cnt >= testing_data.Length) cnt = 0;
            observer.OnNext(testing_data[cnt++]);
        }
    }
}
