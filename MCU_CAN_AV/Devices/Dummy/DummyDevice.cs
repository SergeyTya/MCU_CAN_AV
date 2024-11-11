using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.Shanghai;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        
        private readonly Random _random = new();
        private double _time = 0;
        
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

            _outSpeed = new BaseParameter   { _Min = -12.0f  , _Max = 12.0f   };
            _outTorque = new BaseParameter  { _Min = -300.0f , _Max = 300.0f  };
            _outCurrent = new BaseParameter { _Min = 0.0f    , _Max = 300.0   };
            _outVoltage = new BaseParameter { _Min = 0 };

            DeviceDescription[0].Value.Subscribe((_) =>
            {
                ((BaseParameter)base._outSpeed)._val.OnNext(_);
            });
          

            DeviceDescription[1].Value.Subscribe((_) =>
            {
                ((BaseParameter)base._outTorque)._val.OnNext(_);
            });


            DeviceDescription[2].Value.Subscribe((_) =>
            {
                ((BaseParameter)base._outCurrent)._val.OnNext(_);
            });

            DeviceDescription[3].Value.Subscribe((_) =>
            {
                ((BaseParameter)base._outVoltage)._val.OnNext(_);
            });
            ((BaseParameter)base._outVoltage)._Min = 0;
            ((BaseParameter)base._outVoltage)._Max = 100;
        }


        int cnt = 0;
        int reg = 0;

        void post_fault(ShanghaiDeviceFault fault) {

            base._DeviceFault = fault;

        }

        protected override void Encode(ICAN.RxTxCanData data)
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

                    _time += 0.1;

                    double val = 10 * Math.Sin(0.2*_time) + _random.Next(-1, 1);
                    
                    
                    ((DummyDeviceParameter)DeviceDescription[0]).Val?.OnNext( _random.Next(50, 60) );
                    ((DummyDeviceParameter)DeviceDescription[1]).Val?.OnNext( _random.Next(50, 80) );
                    ((DummyDeviceParameter)DeviceDescription[2]).Val?.OnNext( val );

                    if (!start) base._state = DeviceState.Ready;
                    if (start ) base._state = DeviceState.Run;



                }
                else
                {
                    this.Log().Error("Time out");
                    base._Connection_errors_cnt = _err_cnt++;
                    base._state = DeviceState.NoConnect;

                    post_fault(new ShanghaiDeviceFault { code = 0, name = "fault" });
                }

               TransmitToHardware(new ICAN.RxTxCanData(1, new byte[] { 213, 157 }));

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
            Val.Subscribe(_ => _ValueNow = _);
        }

        internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);

        internal double _ValueNow = 0;

        public IObservable<double> Value => Val;

        public string ID => _prm.ID;

        public string Name => _prm.Name;

        public string Unit => _prm.Unit;

        public double Min => _prm.Min;

        public double Max => _prm.Max;

        public string Type => _prm.Type;

        public List<List<string>> Options => _prm.Options;

        public bool IsReadWrite => _prm.IsReadWrite;

        public double ValueNow => _ValueNow;

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
