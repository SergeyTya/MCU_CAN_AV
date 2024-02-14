using MCU_CAN_AV.Can;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using static MCU_CAN_AV.Devices.Shanghai.ShanghaiDevice;

namespace MCU_CAN_AV.Devices.Dummy
{
    internal class DummyDevice : BaseDevice
    {
        List<ShanghaiDeviceFault> FaultsList = new();

        public DummyDevice()
        {
            List<ShanghaiDeviceParameter> tmp = new();

            try
            {
                string fileContents = MCU_CAN_AV.utils.utils.ReadJsonFromResources(Resources.shanghai_faults);
                FaultsList = JsonConvert.DeserializeObject<List<ShanghaiDeviceFault>>(fileContents);

                fileContents = MCU_CAN_AV.utils.utils.ReadJsonFromResources(Resources.shanghai_description);
                tmp = JsonConvert.DeserializeObject<List<ShanghaiDeviceParameter>>(fileContents);

                base.DeviceDescription.Clear();
                foreach (var item in tmp)
                {
                    base.DeviceDescription.Add(item);
                }
            }
            catch (JsonReaderException e)
            {
                throw new NotImplementedException();
            }
        }


        int cnt = 0;

        public override void Encode(ICAN.RxTxCanData data)
        {
            ((ShanghaiDeviceParameter)DeviceDescription[0]).Val.OnNext(3);

            if (cnt < 5) {

                if (cnt++ == 1)
                {
                    base.Init_stage.OnNext(false);
                    IDevice.Log("Connected!");
                }
                else {
                    IDevice.Log(cnt.ToString());
                }
            }
        }
        public override void Close()
        {
            FaultsList.Clear();   
            DeviceDescription.Clear();
            FaultsList.Clear();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
