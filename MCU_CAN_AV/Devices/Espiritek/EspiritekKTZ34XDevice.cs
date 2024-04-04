using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.Shanghai;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using static MCU_CAN_AV.Devices.Shanghai.ShanghaiDevice;

namespace MCU_CAN_AV.Devices.Espiritek
{


    internal class EspiritekKTZ34XDevice : BaseDevice, IEnableLogger
    {
        internal ICAN.RxTxCanData VCU_transmit = new(0x0800A7A6, new byte[] { 0x65, 0, 0x98, 0x3A, 0x98, 0x3A, 0, 0 }) { NeedUpdate = true }; // 10  ms

        internal List<EspiritekKTZ34XDeviceFault> FaultsList = new();
        private List<IDisposable> Disposable = new();
        private string _name;

        public EspiritekKTZ34XDevice(ICAN CAN) : base(CAN)
        {
            base._Init_stage = false;
            this.Log().Info($"New {nameof(EspiritekKTZ34XDevice)} connection ");
            InitDeviceDescription();

            var disposable = Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe(

                (_) =>
                {
                    if (_ControlEnabled) TransmitToHardware(VCU_transmit);
                    if (VCU_transmit.NeedUpdate)
                    {
                        EncodeData(VCU_transmit);
                        VCU_transmit.NeedUpdate = false;
                    }
                }
            );

            Disposable.Add(disposable);
        }

        protected override void Encode(ICAN.RxTxCanData data)
        {

            if (data.id == 0x0C0AA6A7) _name = $"Espiritek KTZ34X40SI30C01GYSPL MN:[0x{data.data[7].ToString("X")}]";

            if (data.Timeout)
            {
                base._state = DeviceState.NoConnect;
                return;
            }

            EncodeData(data);
            EncodeFaults(data);
        }

        public override string Name => _name;

        private void EncodeFaults(ICAN.RxTxCanData mes)
        {
            int fault_cnt = 0;
            if ((FaultsList.Count > 0) && (mes.id == 0x0C0AA6A7))
            {
                BitArray bits = new BitArray(mes.data);

                for (int i = 0; i < 21; i++)
                {

                    if (bits[i])
                    {

                        foreach (var fault_new in FaultsList)
                        {
                            if (fault_new.adr == i)
                            {
                                base._DeviceFault = fault_new;
                                fault_cnt++;
                            }
                        }
                    }
                }

                if (fault_cnt == 0)
                {
                    if (base.DeviceDescription[11].ValueNow == 1)
                    {
                        base._state = DeviceState.Run;
                    }
                    else {
                        base._state = DeviceState.Ready;
                    }
                }
                else
                {
                    base._state = DeviceState.Fault;
                }



            }
        }

        private void EncodeData(ICAN.RxTxCanData mes)
        {

            BitArray bits = new BitArray(mes.data);

            foreach (EspiritekKTZ34XDeviceParameter item in base.DeviceDescription)
            {
                uint id = 0;
                char[] _trim_hex = new char[] { '0', 'x' };

                if (!uint.TryParse(item.ID.TrimStart(_trim_hex), NumberStyles.HexNumber, null, out id))
                {
                    throw new NotImplementedException();
                };

                if (id == mes.id)
                {

                    var res = utils.utils.CopySlice(bits, item.offset, item.len);
                    int[] val = new int[1];
                    res.CopyTo(val, 0);

                    if (item.Options != null && item.Options.Count > 0)
                    {
                        item.Val.OnNext(val[0]);
                    }
                    else
                    {
                        double vald = ((double)val[0] - (double)item.valoffset) * item.scale;
                        item.Val.OnNext(vald);
                    }
                }
            }
        }


        void InitDeviceDescription()
        {
            List<EspiritekKTZ34XDeviceParameter> tmp = new();

            try
            {
                string fileContents = utils.utils.ReadJsonFromResources(Resources.KTZ34X_faults);
                FaultsList = JsonConvert.DeserializeObject<List<EspiritekKTZ34XDeviceFault>>(fileContents);

                fileContents = utils.utils.ReadJsonFromResources(Resources.KTZ34X_description);
                tmp = JsonConvert.DeserializeObject<List<EspiritekKTZ34XDeviceParameter>>(fileContents);

                base.DeviceDescription.Clear();
                foreach (var item in tmp)
                {
                    item.device = this;
                    base.DeviceDescription.Add(item);
                }


                // speed monitor Need to be in kRPM!
                _outSpeed = new EspiritekKTZ34XDeviceParameter(this)
                {
                    max = 12,
                    min = -12,
                    sname = ((EspiritekKTZ34XDeviceParameter)base.DeviceDescription[7]).Name,
                    CANID = ((EspiritekKTZ34XDeviceParameter)base.DeviceDescription[7]).ID
                };

                Disposable.Add(
                    base.DeviceDescription[7].Value.Subscribe((_) =>
                     {

                         ((EspiritekKTZ34XDeviceParameter)_outSpeed).Val.OnNext(_);
                     }));

                //// current monitor 
                _outCurrent = new EspiritekKTZ34XDeviceParameter(this)
                {
                    max = 50,
                    min = 0,
                    sname = ((EspiritekKTZ34XDeviceParameter)base.DeviceDescription[0]).Name,
                    CANID = ((EspiritekKTZ34XDeviceParameter)base.DeviceDescription[0]).ID
                };

                Disposable.Add(
                    base.DeviceDescription[0].Value.Subscribe((_) =>
                {

                    ((EspiritekKTZ34XDeviceParameter)_outCurrent).Val.OnNext(_);
                }));

                // Torque monitor 
                _outTorque = new EspiritekKTZ34XDeviceParameter(this)
                {
                    max = 2,
                    min = -2.0,
                    sname = ((EspiritekKTZ34XDeviceParameter)base.DeviceDescription[6]).Name,
                    CANID = ((EspiritekKTZ34XDeviceParameter)base.DeviceDescription[6]).ID
                };

               
               base.DeviceDescription[6].Value.Subscribe((_) =>
                {

                    ((EspiritekKTZ34XDeviceParameter)_outTorque).Val.OnNext(_ / base.DeviceDescription[6].Max);

                });

                // Voltage monitor 
                _outVoltage = base.DeviceDescription[1];

                // Speed control
                _inSpeed = base.DeviceDescription[19];

                // Torque control
                _inTorque = base.DeviceDescription[18];

            }
            catch (JsonReaderException e)
            {
                this.Log().Error(e.Message);
            }

        }

        public override void Stop()
        {
            VCU_transmit.data[0] &= 0b00001111;
            VCU_transmit.data[0] |= 0b01100000;
            VCU_transmit.NeedUpdate = true;
            this.Log().Info("Stop command sent");
        }

        public override void Start()
        {
            VCU_transmit.data[0] &= 0b00001111;
            VCU_transmit.data[0] |= 0b01010000;
            VCU_transmit.NeedUpdate = true;
            this.Log().Info("Start command sent");
        }

        public override void Reset()
        {

            byte tmp = VCU_transmit.data[0];
            tmp &= 0x0f;

            VCU_transmit.data[0] &= 0b00001111;
            VCU_transmit.data[0] |= 0b00100000;
            VCU_transmit.NeedUpdate = true;
            this.Log().Info("Reset command sent");
        }

        public override void Close_instance()
        {
            Disposable.ForEach(x => x?.Dispose());
            Disposable.Clear();
        }


        internal class EspiritekKTZ34XDeviceFault : IDeviceFault, IEnableLogger
        {
           
            public string ID => adr.ToString();

            public string Name => name;

            [JsonProperty("adr")]
            internal uint adr;

            [JsonProperty("name")]
            internal string name;
        }

        internal class EspiritekKTZ34XDeviceParameter : IDeviceParameter, IEnableLogger
        {
            internal EspiritekKTZ34XDevice device;
            public EspiritekKTZ34XDeviceParameter(EspiritekKTZ34XDevice device):base()
            {
                this.device = device;
                Value.Subscribe((_) => _valueNow = _);
            }

            internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);

            public IObservable<double> Value { get => Val; }

            public string ID { get => CANID; }

            public string Name { get => sname; }

            public string Unit { get => unit; }

            public double Min { get => min; }

            public double Max { get => max; }

            public string Type { get => null; }

            public List<List<string>> Options { get => options; }

            public bool IsReadWrite { get => RW; }


            private double _valueNow = 0;
            public double ValueNow => _valueNow;

            public void writeValue(double value)
            {
                this.Log().Info($"{ID} offset {offset} len {len} <- {value}");

                Int32[] val_int;
                if (Options != null && Options.Count > 0)
                {
                    val_int = new Int32[] { (Int32)value };
                }
                else
                {
                    val_int = new Int32[] { (Int32)((value + (double)valoffset) * scale) };
                }

                BitArray dst = new BitArray(device.VCU_transmit.data);
                BitArray src = new BitArray(val_int);
                utils.utils.CopySliceTo(dst, offset, src, 0, len);
                dst.CopyTo(device.VCU_transmit.data, 0);
                device.VCU_transmit.NeedUpdate = true;

            }

            [JsonProperty("CANID")]
            internal string CANID;

            [JsonProperty("pos")]
            internal uint pos;

            [JsonProperty("len")]
            internal int len;

            [JsonProperty("offset")]
            internal int offset;

            [JsonProperty("valoffset")]
            internal int valoffset;
           

            [JsonProperty("scale")]
            internal float scale { get; set; }

            [JsonProperty("sname")]
            internal string sname { get; set; }

            [JsonProperty("options")]
            internal List<List<string>> options { get; set; }

            [JsonProperty("RW")]
            internal bool RW { get; set; }

            [JsonProperty("min")]
            internal double min { get; set; }

            [JsonProperty("max")]
            internal double max { get; set; }

            [JsonProperty("unit")]
            internal string unit { get; set; }



        }

    }


}
