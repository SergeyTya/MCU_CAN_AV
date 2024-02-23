using ScottPlot.Renderable;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.utils
{
    public class LogProvider : ILogProvider
    {
        Subject<string> _log = new();
        public IObservable<string> GetObservable => _log;

        public void Post(string? message)
        {
            if (message == null) return;
            _log.OnNext(message);
        }
    }
}
