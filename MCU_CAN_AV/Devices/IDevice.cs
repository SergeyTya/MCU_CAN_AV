using Avalonia.Logging;
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
        ShanghaiCAN,
        EspiritekCAN,
        Dummy
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
        static Subject<string> _logUpdater = new Subject<string>();
        static IDevice _Device = new BaseDevice();

        public static ObservableCollection<IDeviceParameter> _DeviceDescription = new();
        public static ObservableCollection<IDeviceFault> _DeviceFaults = new();

        public ObservableCollection<IDeviceParameter> DeviceDescription { get; }
        public ObservableCollection<IDeviceFault> DeviceFaults { get; }

        public BehaviorSubject<bool> Init_stage { get; }

        /// <summary>
        /// Close connection
        /// </summary>
        void Close();
        /// <summary>
        /// Encode message from ICAN hardware
        /// </summary>
        /// <param name="data"></param>
        void Encode(ICAN.RxTxCanData data);
        
        /// <summary>
        /// Send reset to device
        /// </summary>
        public void Reset();
        /// <summary>
        /// Send Start to device
        /// </summary>
        public void Start();
        /// <summary>
        /// Send Stop to device
        /// </summary>
        public void Stop();

        /// <summary>
        /// Get device instatnce
        /// </summary>
        /// <returns>
        /// IDevice object
        /// </returns>
        public static IDevice GetInstnce() {
            return IDevice._Device;
        }

        /// <summary>
        /// Close connection and dispose IDevice instatnce
        /// </summary>
        public static void Dispose(){
            IDevice._Device.Close();
            ICAN.Dispose();
            IDevice._logUpdater.OnNext($" Connection closed ");
            IDevice._Device = null;
        }

        /// <summary>
        /// Create new IDevice instatnce and start new connection
        /// </summary>
        /// <param name="device"></param>
        /// <param name="InitStruct"></param>
        /// <returns></returns>
        public static IDevice Create(DeviceType device, ICAN.CANInitStruct InitStruct) {

            IDevice? ret_obj = null;

            IDisposable loglistener = ICAN.LogUpdater.Subscribe(
                      (_) => {
                          IDevice._logUpdater.OnNext(_);
                      });

            switch (device)
            {
                case DeviceType.EVMModbus:
                    ret_obj = new EVMModbusDevice(InitStruct);
                    ICAN.Create(InitStruct, ICAN.CANType.ModbusTCP);
                    break;
                case DeviceType.ShanghaiCAN:
                    ret_obj = new ShanghaiDevice();
                    ICAN.Create(InitStruct, ICAN.CANType.CAN_USBCAN_B);
                    break;
                case DeviceType.EspiritekCAN:
                    ICAN.Create(InitStruct, ICAN.CANType.CAN_USBCAN_B);
                    break;
                case DeviceType.Dummy:
                    ret_obj = new DummyDevice();
                    ICAN.Create(InitStruct, ICAN.CANType.Dummy);
                    break;
            }

            if (ret_obj != null)
            {
                IDevice.Log($" !!!! {ret_obj.GetType().Name} connection created ");
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
                            ICAN.CAN?.Transmit(_);
                        });
                    });

            }

           IDevice._Device = ret_obj;

           return GetInstnce();
        }

        /// <summary>
        /// Static IDevice observable logger
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>

        public static IDisposable SubscribeToLogger(Action<string> action)
        {

            return _logUpdater.Subscribe(action);
        }

        /// <summary>
        /// Log message via static IDevice observable logger
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            _logUpdater.OnNext(message);
        }

    }
}
