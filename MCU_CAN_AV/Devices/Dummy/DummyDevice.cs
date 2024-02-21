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

        void post_fault(ShanghaiDeviceFault fault) {

            base._DeviceFault = fault;

        }

        public override void Encode(ICAN.RxTxCanData data)
        {
            post_fault(new ShanghaiDeviceFault { code = 0, name = "DC ok"  });
            post_fault(new ShanghaiDeviceFault { code = 0, name = "Fault1" });
            post_fault(new ShanghaiDeviceFault { code = 0, name = "Fault2" });

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
            else
            {

                if (!fault)
                {
                  
                    ((DummyCAN)ICAN.CAN).faultmode = false;
                    ((ShanghaiDeviceParameter)DeviceDescription[0]).Val.OnNext(reg++);
                    ((ShanghaiDeviceParameter)DeviceDescription[1]).Val.OnNext(reg++);
                    ((ShanghaiDeviceParameter)DeviceDescription[2]).Val.OnNext(reg++);

                    if (!start) base._state = DeviceState.Ready;
                    if (start) base._state = DeviceState.Run;

                }
                else
                {
                    IDevice.Log("Time out");
                    ((DummyCAN)ICAN.CAN).faultmode = true;
                    base._Connection_errors_cnt = _err_cnt++;
                    base._state = DeviceState.NoConnect;

                    post_fault(new ShanghaiDeviceFault { code = 0, name = "fault" });
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
           // base.DeviceFaults.Clear();
            IDevice.Log("Reset command");
        }

        public override void Start()
        {
            start = true;
            IDevice.Log("Start command");
            post_fault(new ShanghaiDeviceFault { code = 0, name = "Run" });
        }

        public override void Stop()
        {
            start = false;
            IDevice.Log("Stop command");
            post_fault(new ShanghaiDeviceFault { code = 0, name = "Ready" });
        }
    }
}
