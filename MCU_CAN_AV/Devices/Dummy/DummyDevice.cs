using MCU_CAN_AV.Can;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
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
        bool fault = false;
        bool start = false;

        int _err_cnt = 0;
 

        public override string Name => "Dummy device";
      

        List<ShanghaiDeviceFault> FaultsList = new();

        public DummyDevice()
        {
            base._Connection_errors_cnt = 0;
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
        int reg = 0;

        public override void Encode(ICAN.RxTxCanData data)
        {
            
            if (cnt < 5)
            {

                if (cnt++ == 1)
                {
                    base._Init_stage = false;
                    IDevice.Log("Connected!");
                }
                else
                {
                    IDevice.Log(cnt.ToString());
                }
            }
            else {

                if (!fault)
                {
                    base.DeviceFaults.Clear();
                    ((DummyCAN)ICAN.CAN).faultmode = false;
                    ((ShanghaiDeviceParameter)DeviceDescription[0]).Val.OnNext(reg++);
                    ((ShanghaiDeviceParameter)DeviceDescription[1]).Val.OnNext(reg++);
                    ((ShanghaiDeviceParameter)DeviceDescription[2]).Val.OnNext(reg++);

                   if(!start) base._state = DeviceState.Ready;
                   if(start) base._state = DeviceState.Run;
                    if (base.DeviceFaults.Count == 0)
                    {
                        base.DeviceFaults.Add(new ShanghaiDeviceFault { code = 0, name = "DC ok" });
                    }
                }
                else
                {
                    IDevice.Log("Time out");
                    ((DummyCAN)ICAN.CAN).faultmode = true;
                    base._Connection_errors_cnt = _err_cnt++;
                    base._state = DeviceState.NoConnect;

                    if (base.DeviceFaults.Count <2) {
                        base.DeviceFaults.Add( new ShanghaiDeviceFault { code=0, name = "test fault"});
                    }
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
            fault = !fault;
            start = false;
            IDevice.Log("Reset command");
        }

        public override void Start()
        {
            start = true;
            IDevice.Log("Start command");
        }

        public override void Stop()
        {
           start = false;
            IDevice.Log("Stop command");
        }
    }
}
