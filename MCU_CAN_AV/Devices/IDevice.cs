using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        public string Min { get; }

        public string Max { get; }

        public string Type { get; }

        public List<string> Options { get; } // parameter options

        public bool IsReadWrite { get; } 

    }

    public interface IDeviceFault {
        public string ID { get; }  
        public string Name { get; }  
    }

    internal interface IDevice
    {
        internal static IDevice Device;

        public static ObservableCollection<IDeviceParameter> DeviceDescription = new();
        public static ObservableCollection<IDeviceFault> DeviceFaults = new();

        public static string ReadJsonFromResources(byte[] res)
        {
            MemoryStream MS = new MemoryStream(res);
            StreamReader sr = new StreamReader(MS);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            return fileContents;
        }
       
        void Reset();


        public void ResetStatic() {
            Device.Reset();
        }
        
    }
}
