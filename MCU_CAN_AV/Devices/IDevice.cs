using Avalonia.Threading;
using MCU_CAN_AV.Can;
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
        public enum DeviceType { 
            EVMModbus,
            Shanghai,
            Espiritek
        
        }
        public static IDevice? Device;
        public static ObservableCollection<IDeviceParameter> DeviceDescription = new();
        public static ObservableCollection<IDeviceFault> DeviceFaults = new();
        public static Subject<string> logUpdater = new Subject<string>();

        public static string ReadJsonFromResources(byte[] res)
        {
            MemoryStream MS = new MemoryStream(res);
            StreamReader sr = new StreamReader(MS);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            return fileContents;
        }
        void Encode(ICAN.RxTxCanData data);
        void Reset();
        void Start();
        void Stop();

        public static void ResetStatic() {
            if(Device != null)
            {
                Device.Reset();
            }
            
        }

        public static void StartStatic()
        {
            if (Device != null)
            {
                Device.Start();
            }
        }

        public static void StopStatic()
        {
            if (Device != null)
            {
                Device.Stop();
            }
        }

        public static void Create(DeviceType device, ICAN.CANInitStruct InitStruct) {

            IDevice.logUpdater.Subscribe(_ => Debug.WriteLine(_));

            switch (device)
            {
                case DeviceType.EVMModbus:
                    IDevice.Device = new EVMModbusDevice(InitStruct);
                    break;
                case DeviceType.Shanghai:
                    IDevice.Device = new ShanghaiDevice();
                    break;
                case DeviceType.Espiritek:

                    break;
            }

            if(Device != null)
            {
                ICAN.Create(InitStruct);

                if (ICAN.CAN != null) {

                    IDisposable loglistener = ICAN.LogUpdater.Subscribe(
                        (_) => {
                            IDevice.logUpdater.OnNext(_);
                        });

                    IDisposable Rxlistener = ICAN.RxUpdater.Subscribe(
                        (_) =>
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                IDevice.Device.Encode(_);
                            });
                        });

                    IDisposable Txlistener = ICAN.TxUpdater.Subscribe(
                        (_) =>
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ICAN.CAN.Transmit(_);
                            });
                        });
                }
            }
        }
    }
}
