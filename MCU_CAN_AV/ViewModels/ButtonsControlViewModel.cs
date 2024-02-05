using MCU_CAN_AV.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{
    internal class ButtonsControlViewModel
    {
        public void onClickResetButton() {

            IDevice.ResetStatic();
        }
        public void onClickStartButton()
        {

            IDevice.StartStatic();
        }

        public void onClickStopButton() { 

            IDevice.StopStatic();
        }

    }
}
