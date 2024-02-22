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
    internal class LogService : ILogger, ILogService
    {
        Subject<string> _log  = new();

        void log(string mes) {

            _log.OnNext(mes);
        }

        public void Write(string message, LogLevel logLevel)
        {
            if ((int)logLevel < (int)Level) return;
            log(message);
        }

        void ILogger.Write(Exception exception, string message, LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        void ILogger.Write(string message, Type type, LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        void ILogger.Write(Exception exception, string message, Type type, LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public LogLevel Level { get; set; }

        public IObservable<string> GetObservable => _log;
    }
}
