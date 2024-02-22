using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.utils
{
    public interface ILogService
    {
       IObservable<string> GetObservable { get; }

    }
}
