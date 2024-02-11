using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal class ButtonsControlViewModel: ViewModelBase
    {
        public void onClickResetButton() => IDevice.GetInstnce()?.Reset();
        
        public void onClickStartButton() => IDevice.GetInstnce()?.Start();
        
        public void onClickStopButton() => IDevice.GetInstnce()?.Stop();
        

    }
}
