using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Models
{
    internal class MainModel
    {
        
        public MainModel() {
            DeviceDescriprion.DeviceDescriptionReader.Read();
        }
    }
}
