using System;

namespace MCU_CAN_AV.Can
{
    public partial interface ICAN
    {
        class ICANException : Exception
        {
            public ICANException(string message) : base(message) { }
        }
    }
}
