using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices
{
    public interface IDeviceParameter
    {

        public void writeValue(double value);  // write value to device

        public IObservable<double> Value { get; } // observable parameter

        public string ID { get; }  // parameter ID

        public string Name { get; } // parameter descriprion

        public string Unit { get; }

        public double Min { get; }

        public double Max { get; }

        public string Type { get; }

        public List<List<string>> Options { get; } // parameter options

        public bool IsReadWrite { get; }

    }
}
