using Avalonia.Threading;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.Dummy;
using MCU_CAN_AV.Devices.EVM_DIAG;
using MCU_CAN_AV.Devices.Shanghai;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;


namespace MCU_CAN_AV.Devices
{

    internal interface IDevice
    {

        private static IDevice? _Device;
        
        /// <summary>
        ///  Collection of IDeviceParameters
        /// </summary>
        public ObservableCollection<IDeviceParameter>        DeviceDescription     { get; }

        /// <summary>
        ///     Posting IDevice Faults
        /// </summary>
        public IObservable<IDeviceFault>                     DeviceFaults          { get; }

        /// <summary>
        ///   Connection stage finished (posted false)
        /// </summary>
        public IObservable<bool>                             Init_stage            { get; }

        /// <summary>
        ///     Observable for device state
        /// </summary>
        public IObservable<string>                           State                 { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public IObservable<int>                              Connection_errors_cnt { get; }

        /// <summary>
        ///     Provide device name
        /// </summary>
        public string                                        Name                  { get; }


        /// <summary>
        ///     Observable to read Rx data from hardware
        /// </summary>
        public IObservable<ICAN.RxTxCanData>                 RxData                { get; }

        /// <summary>
        ///     Observable to read Tx data from hardware
        /// </summary>
        public IObservable<ICAN.RxTxCanData>                 TxData                { get; }


        //******************************* Controls ***********************************

        IDeviceParameter OutVoltage { get; }
        IDeviceParameter OutCurrent { get; }
        IDeviceParameter OutTorque  { get; }
        IDeviceParameter OutSpeed   { get; }

        IDeviceParameter InTorque   { get; }
        IDeviceParameter InSpeed    { get; }

        //****************************************************************************


        /// <summary>
        /// Close connection
        /// </summary>
        void Close();
        
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
        /// Get IDevice instance
        /// </summary>
        /// <returns>
        /// IDevice object
        /// </returns>
        public static IDevice Current { get => IDevice._Device; }

        /// <summary>
        /// Close connection and dispose IDevice instatnce
        /// </summary>
        public static void Dispose(){
            IDevice._Device?.Close();
        }

        /// <summary>
        /// Create new IDevice instatnce and start new connection
        /// </summary>
        /// <param name="device"></param>
        /// <param name="InitStruct"></param>
        /// <returns></returns>
        /// 
        public static IDevice Create( DeviceType device, ICAN.CANInitStruct InitStruct) {

            IDevice? ret_obj = null;
            ICAN? CAN = null;

            switch (device)
            {
                case DeviceType.EVMModbus:

                    CAN = ICAN.Create(InitStruct, ICAN.CANType.ModbusTCP);
                    if(CAN!=null) ret_obj = new EVMModbusDevice(CAN);
                    break;
                case DeviceType.ShanghaiCAN:
                    CAN = ICAN.Create(InitStruct, ICAN.CANType.CAN_USBCAN_B);
                    if (CAN != null) ret_obj = new ShanghaiDevice(CAN);
                    break;
                case DeviceType.Dummy:
                    CAN = ICAN.Create(InitStruct, ICAN.CANType.Dummy);
                    if (CAN != null)  ret_obj = new DummyDevice(CAN);
                    break;
            }

           IDevice._Device = ret_obj;
           return Current;
        }

    }
}
