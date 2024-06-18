using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.utils
{
    internal interface IDataLogger
    {
        void start(IDevice Device);
        void pause();
        void resume();
        void close();
        void openPath();

    }
}
