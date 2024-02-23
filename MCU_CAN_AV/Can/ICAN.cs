using MCU_CAN_AV.Devices.Dummy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Can
{
    public partial interface ICAN
    {

        /// <summary>
        ///     Defined can init structure
        /// </summary>
        /// <returns>
        ///     CANInitStruct
        /// </returns>
        public CANInitStruct InitStructure { get; }


        /// <summary>
        ///     Is can transport open
        /// </summary>
        /// <returns>
        ///     bool
        /// </returns>
        public bool isOpen { get; }

        /// <summary>
        ///     Post input RX data from hardware
        /// </summary>
        /// <returns>
        ///     IObservable
        /// </returns> 
        public IObservable<ICAN.RxTxCanData> RxObservable { get; }


        /// <summary>
        ///     Transmit RxTxCanData object to hardware
        /// </summary>
        public void Transmit(RxTxCanData data);

        /// <summary>
        ///  Provide device registers map
        /// </summary>
        /// <returns></returns>
        public byte[][] ReadDeviceDescriptionsBytes();

        /// <summary>
        ///  Provide device info 
        /// </summary>
        /// <returns></returns>
        public byte[] ReadDeviceInfoBytes();

        /// <summary>
        ///     Close hardware transport and dispose all objects
        /// </summary>
        public void Close();

        /// <summary>
        ///    CAN Factory
        /// </summary>
        ///  <returns>
        ///    ICAN object
        /// </returns>
        public static ICAN? Create(CANInitStruct InitStructure, CANType CANType)
        {

            ICAN? ret_val = null;

            switch (CANType)
            {
                case CANType.ModbusTCP:

                    ret_val = new ModbusTCP.ModbusTCP(InitStructure);
                    break;

                case CANType.CAN_USBCAN_B:
                    ret_val = new USBCAN_B_win(InitStructure);
                    break;

                case CANType.Dummy:
                    ret_val = new DummyCAN(InitStructure);
                    break;
            }

            return ret_val; 
        }
    }
}
