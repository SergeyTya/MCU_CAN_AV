
using System;
using System.Reactive.Subjects;

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
