using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.utils
{
    internal class StaticLogger
    {
        static Subject<string> _LogUpdater = new Subject<string>();


        public static void Log(string message) {

            _LogUpdater.OnNext($"{DateTime.Now}: {message} \n");
        }

        public static IDisposable Subscribe( Action<string> action)
        {

            return _LogUpdater.Subscribe(action);
        }
    }
}
