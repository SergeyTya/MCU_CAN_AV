using Avalonia.Threading;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.Dummy;
using MCU_CAN_AV.Devices.EVM_DIAG;
using MCU_CAN_AV.Devices.Shanghai;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices
{
    public enum DeviceType
    {
        EVMModbus,
        Shanghai,
        Espiritek

    }

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

        public List<string> Options { get; } // parameter options

        public bool IsReadWrite { get; } 

    }

    public interface IDeviceFault {
        public string ID { get; }  
        public string Name { get; }  
    }

    internal interface IDevice
    {
        static IDevice _Device = new BaseDevice();

        public Subject<string> LogUpdater { get; }

        public ObservableCollection<IDeviceParameter> DeviceDescriprion { get; }
        public ObservableCollection<IDeviceFault> DeviceFaults { get; }
      
        public BehaviorSubject<bool> Init_stage { get;  }

        void Encode(ICAN.RxTxCanData data);
        public void Reset();
        public void Start();
        public void Stop();

        public static IDevice GetInstnce() {
            return IDevice._Device;
        }
        public static IDevice Create(DeviceType device, ICAN.CANInitStruct InitStruct) {

            IDevice? ret_obj = null;

            ret_obj = new DummyDevice();

            //switch (device)
            //{
            //    case DeviceType.EVMModbus:
            //        ret_obj = new EVMModbusDevice(InitStruct);
            //        break;
            //    case DeviceType.Shanghai:
            //        ret_obj = new ShanghaiDevice();
            //        break;
            //    case DeviceType.Espiritek:

            //        break;
            //}

            if (ret_obj != null)
            {
                ret_obj.LogUpdater.Subscribe(_ => Debug.WriteLine(_));

                ICAN.Create(InitStruct);

                if (ICAN.CAN != null) {

                    IDisposable loglistener = ICAN.LogUpdater.Subscribe(
                        (_) => {
                            ret_obj.LogUpdater.OnNext(_);
                        });

                    IDisposable Rxlistener = ICAN.RxUpdater.Subscribe(
                        (_) =>
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                ret_obj.Encode(_);
                            });
                        });

                    IDisposable Txlistener = ICAN.TxUpdater.Subscribe(
                        (_) =>
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                ICAN.CAN.Transmit(_);
                            });
                        });
                }
            }

           IDevice._Device = ret_obj;

           return GetInstnce();
        }

        public static string ReadJsonFromResources(byte[] res)
        {
            MemoryStream MS = new MemoryStream(res);
            StreamReader sr = new StreamReader(MS);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            return fileContents;
        }
    }
}
