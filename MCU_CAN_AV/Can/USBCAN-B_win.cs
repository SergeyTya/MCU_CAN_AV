using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace MCU_CAN_AV.Can
{

    /*------------ ZLG-compatible data types---------------------------------*/

    //1.The data type of ZLGCAN series interface card information.
    public struct VCI_BOARD_INFO
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Reserved;
    }

    /////////////////////////////////////////////////////
    //2.Defines the data type of the CAN information frame.
    unsafe public struct VCI_CAN_OBJ  //Use unsafe code
    {
        public uint ID;
        public uint TimeStamp;        //Time stamp
        public byte TimeFlag;         //Whether to use time stamp
        public byte SendType;         //Send flag.Reserved, unused
        public byte RemoteFlag;       //Is it a remote frame
        public byte ExternFlag;       //Whether it is an extended frame
        public byte DataLen;          //Data length
        public fixed byte Data[8];    //Data
        public fixed byte Reserved[3];//Reserved bit

    }

    //3.Define the data type that initializes the CAN
    public struct VCI_INIT_CONFIG
    {
        public UInt32 AccCode;
        public UInt32 AccMask;
        public UInt32 Reserved;
        public byte Filter;   //0 or 1 receives all frames.2 Standard frame filtering, 3 is extended frame filtering.
        public byte Timing0;  //Baud rate parameters, for specific configuration, please check the function manual of the secondary development library
        public byte Timing1;
        public byte Mode;     //Mode, 0 means normal mode, 1 means listen only mode, 2 self-test mode
    }

    /*------------ Description of other data structures---------------------------------*/
    //4.The data type of the USB-CAN bus adapter board information is 1, which is the return parameter of the VCI_FindUsbDevice function.
    public struct VCI_BOARD_INFO1
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        public byte Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_Usb_Serial;
    }

    /*------------ Data structure description completed---------------------------------*/

    public struct CHGDESIPANDPORT
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] szpwd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] szdesip;
        public Int32 desport;

        public void Init()
        {
            szpwd = new byte[10];
            szdesip = new byte[20];
        }
    }

    internal class USBCAN_B_win : BaseCAN, IEnableLogger
    {
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);

        //***********************************************************************************/

        UInt32[] m_arrdevtype = new UInt32[20];

        VCI_CAN_OBJ[] m_recobj = new VCI_CAN_OBJ[1000];



        UInt32 m_devtype = 4; //USBCAN2


        bool _isOpen = false;


        public USBCAN_B_win(ICAN.CANInitStruct init) : base(init)
        {
            InitCAN(InitStructure);
        }
       
        //************************************************************************************/


        public void InitCAN(ICAN.CANInitStruct init)
        {
          
            UInt32 m_devind = InitStructure._devind;
            UInt32 m_canind = InitStructure._canind;
            

            if (_isOpen)
            {
                VCI_CloseDevice(m_devtype, m_devind);
                _isOpen = false;
            }

            if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
            {

                this.Log().Error(String.Format("Unable connect to USBCAN-B adapter DevType={0}, DevInd={1}", m_devtype, m_devind));
                return;
            }

         
            VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();

            config.AccCode = InitStructure._RcvCode;
            config.AccMask = InitStructure._Mask;

            //00, 1c - 500k
            //01, 1c - 250k

            byte Timing1 = 0x00;
            byte Timing2 = 0x1c;

            if (InitStructure._Baudrate == 250) {
                Timing1 = 1;
            }

            config.Timing0 = Timing1;
            config.Timing1 = Timing2;

            config.Filter = 1; // 1 - All , 2 - Standart, 3 - Extended
            config.Mode = 0;  // 0 - Normal, 1 - Listener, 2-Echo
            VCI_InitCAN(m_devtype, m_devind, m_canind, ref config);
            VCI_StartCAN(m_devtype, InitStructure._devind, InitStructure._canind);
            _isOpen = true;
            this.Log().Info($"{nameof(USBCAN_B_win)} connection open");
        }
        int TimeOut_counter = 0;

    
        public unsafe override void Receive()
        {
          
            UInt32 res = VCI_Receive(m_devtype, InitStructure._devind, InitStructure._canind, ref m_recobj[0], 1000, 100);

            if (res == 0xffffffff)
            {
                this.Log().Error("USBCAN Recieve Error");
                post(new ICAN.RxTxCanData() { Timeout = true });
                return;
            }

            if(res == 0) {
                TimeOut_counter++;
                if (TimeOut_counter > 100) {
                    this.Log().Error("USBCAN_B Recieve Timeout");
                    post(new ICAN.RxTxCanData() { Timeout = true });
                    TimeOut_counter = 0;
                    return;
                }
                
            }

            for (UInt32 i = 0; i < res; i++)
            {
                fixed (VCI_CAN_OBJ* m_recobj1 = &m_recobj[i])
                {

                    byte[] dtr = new byte[8];
                    for (int j = 0; j < 8; j++)
                    {
                        dtr[j] = m_recobj[i].Data[j];
                    }

                   post(new ICAN.RxTxCanData(m_recobj[i].ID, dtr));
                }
            }
        }

        public override void Close_instance()
        {
            VCI_ResetCAN(m_devtype, InitStructure._devind, InitStructure._canind);
            _isOpen = false;
            this.Log().Info($"{nameof(USBCAN_B_win)} connection closed");
            base.Dispose();
        }

        public override bool isOpen { get => _isOpen; }


        public override void Transmit(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }
    }
}
