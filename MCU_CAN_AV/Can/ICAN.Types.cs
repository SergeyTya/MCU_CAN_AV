namespace MCU_CAN_AV.Can
{
    public partial interface ICAN
    {
        public enum CANType
        {
            CAN_USBCAN_B = 0,
            ModbusTCP = 1,
            ModbusRTU = 2,
            Dummy = 3,
            PCAN_USB
        }
    }
}
