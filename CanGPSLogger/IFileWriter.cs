using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanGPSLogger
{
    internal interface IFileWriter
    {
        void LogCanMessage();
        void LogPosition();

    }
}
