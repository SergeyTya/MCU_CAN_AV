﻿using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.Shanghai;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using static MCU_CAN_AV.Devices.Shanghai.ShanghaiDevice;

namespace MCU_CAN_AV.Devices.Dummy
{
    internal class DummyDevice : BaseDevice, IEnableLogger
    {
        bool fault = false;
        bool start = false;

        int _err_cnt = 0;


        public override string Name => "Dummy device";


        List<ShanghaiDeviceFault> FaultsList = new();

        public DummyDevice(ICAN CAN):base(CAN)
        {
            this.Log().Info($"New {nameof(DummyDevice)} connection ");
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
                    var el = new DummyDeviceParameter(item);
                    base.DeviceDescription.Add(el);
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
                    this.Log().Info("Connected!");
                }
                else
                {
                    this.Log().Info(cnt.ToString());
                }
            }
            else
            {

                if (!fault)
                {
                    if (DeviceDescription == null) return;
                    if (DeviceDescription.Count <= 0) return;
                    ((DummyCAN) _CAN).faultmode = false;
                    ((DummyDeviceParameter)DeviceDescription[0]).Val?.OnNext(reg++);
                    ((DummyDeviceParameter)DeviceDescription[1]).Val?.OnNext(reg++);
                    ((DummyDeviceParameter)DeviceDescription[2]).Val?.OnNext(reg++);

                    if (!start) base._state = DeviceState.Ready;
                    if (start ) base._state = DeviceState.Run;

                }
                else
                {
                    this.Log().Error("Time out");
                    ((DummyCAN) _CAN).faultmode = true;
                    base._Connection_errors_cnt = _err_cnt++;
                    base._state = DeviceState.NoConnect;

                    post_fault(new ShanghaiDeviceFault { code = 0, name = "fault" });
                }

            }
        }

        public override void Close_instance()
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
            this.Log().Info("Reset command");
        }

        public override void Start()
        {
            start = true;
            this.Log().Info("Start command");
            post_fault(new ShanghaiDeviceFault { code = 0, name = "Run" });
        }

        public override void Stop()
        {
            start = false;
            this.Log().Info("Stop command");
            post_fault(new ShanghaiDeviceFault { code = 0, name = "Ready" });
        }
    }

    internal class DummyDeviceParameter : IDeviceParameter, IEnableLogger
    {
        ShanghaiDeviceParameter _prm;

        public DummyDeviceParameter(ShanghaiDeviceParameter prm) {
            _prm = prm;
        }

        internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);

        public IObservable<double> Value => Val;

        public string ID => _prm.ID;

        public string Name => _prm.Name;

        public string Unit => _prm.Unit;

        public double Min => _prm.Min;

        public double Max => _prm.Max;

        public string Type => _prm.Type;

        public List<List<string>> Options => _prm.Options;

        public bool IsReadWrite => _prm.IsReadWrite;

        public void writeValue(double value)
        {
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                this.Log().Info($"{ID} <- {value.ToString()} ");
                Val.OnNext(value);
            });
        }



    }
}
