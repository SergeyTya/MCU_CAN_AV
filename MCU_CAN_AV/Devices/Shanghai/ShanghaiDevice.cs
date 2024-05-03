using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.EVM_DIAG;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScottPlot.Colormaps;
using SkiaSharp;
using Splat;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using static MCU_CAN_AV.Devices.EVM_DIAG.EVMModbusDevice;

namespace MCU_CAN_AV.Devices.Shanghai
{
    internal class ShanghaiDevice : BaseDevice, IEnableLogger
    {
        private string _name = "";
        internal List<ShanghaiDeviceFault> FaultsList = new();

        System.Timers.Timer RxTimer;

        private enum CAN_IDs
        {
            FAULT_ID = 0x18F39CD1,
            STATE1_ID = 0x18F1F2D1,
            STATE2_ID = 0x18F120D1,
            SW_ID = 0x18F121D1,
            ID_MC1 = 0xCF10AD0,
            ID_MC3 = 0xCF12AD0
        }

        internal  ICAN.RxTxCanData Tx_MC1 = new(0xCF10AD0, new byte[] { 0x30, 0x75, 0x84, 0x4E, 0x43, 0x10, 0x00, 0x00 }) { NeedUpdate = true }; // 10  ms
        internal  ICAN.RxTxCanData Tx_MC2 = new(0xC50A4D0, new byte[] { 0xAA, 0xb8, 0x0b, 0xAA, 0x64, 0x00, 0x00, 0x00 }) { NeedUpdate = true }; // 100 ms
        internal  ICAN.RxTxCanData Tx_MC3 = new(0xCF12AD0, new byte[] { 0x60, 0xEA, 0x00, 0x00, 0xB4, 0x5F, 0x00, 0x00 }) { NeedUpdate = true }; // 100 ms

        Subject<ICAN.RxTxCanData> TxSubject = new Subject<ICAN.RxTxCanData>();
     
        CancellationTokenSource cts = new CancellationTokenSource();
       
        private SemaphoreSlim _semaphore = new(1);
 
        private IDisposable? TxDisposable;
        private IDisposable? Disposable_MS1;
        private IDisposable? Disposable_MS2;
        private IDisposable? Disposable_MS3;

       

        ///
        public ShanghaiDevice(ICAN CAN) : base(CAN)
        {
            this.Log().Info($"New {nameof(ShanghaiDevice)} connection ");
            InitDeviceDescription();
            base._Init_stage = false;

            CancellationToken token = cts.Token;

            TxDisposable = TxSubject.Subscribe(async (_) =>
            {

                await _semaphore.WaitAsync(token);
                try
                {
                   if(_ControlEnabled) TransmitToHardware(_);
                   if(_.NeedUpdate)EncodeData(_);
                    _.NeedUpdate = false;
                }
                finally
                {
                    _semaphore.Release();
                }

            });

            byte msg1_cnt = 1;
            Disposable_MS1 = Observable.Interval(TimeSpan.FromMilliseconds(10)).Subscribe((_) =>
            {

                Tx_MC1.data[6] = (byte) ((msg1_cnt & (byte) 0x0F) + ( Tx_MC1.data[6] & (byte)0xF0));
                if (msg1_cnt++ == 16) msg1_cnt = 1;
                if (!TxSubject.IsDisposed) TxSubject.OnNext(Tx_MC1);

            });

            byte msg2_cnt = 1;
            Disposable_MS2 = Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe((_) =>
            {
                Tx_MC2.data[7] = msg2_cnt;
                if (msg2_cnt++ == 0) msg2_cnt = 1;
                if (!TxSubject.IsDisposed) TxSubject.OnNext(Tx_MC2);
                
            });

            Disposable_MS3 = Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe((_) =>
            {
                if (!TxSubject.IsDisposed) TxSubject.OnNext(Tx_MC3);
            });
        }


        private void EncodeData(ICAN.RxTxCanData mes)
        {

            BitArray bits = new BitArray(mes.data);

            foreach (ShanghaiDeviceParameter item in base.DeviceDescription)
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
                        double vald = ((double)val[0] * (double)item.scale - (double)item.valoffset);
                        item.Val.OnNext(vald);
                    }
                }

            }
        }

        private void EncodeFaults(ICAN.RxTxCanData mes)
        {

            if ((FaultsList.Count > 0) && (mes.id == (uint)0x18F39CD1))
            {
                foreach (var fault_new in FaultsList)
                {
                    if (fault_new.code == mes.data[2])
                    {
                        base._DeviceFault = fault_new;

                    }

                    if (fault_new.code == mes.data[5])
                    {
                        base._DeviceFault = fault_new;

                    }
                }

                if (mes.data[0] == 0)
                {
                    if (base.DeviceDescription[11].ValueNow != 0)
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

     

        public override void Close_instance()
        {
            cts.Cancel();
            TxSubject?.Dispose();
            TxDisposable?.Dispose();    
            Disposable_MS1?.Dispose();
            Disposable_MS2?.Dispose();
            Disposable_MS2?.Dispose();
        }

        public override void Reset()
        {

            Tx_MC1.data[4] &= 0b00011111; // reset flags
            Tx_MC1.data[4] |= 0b01000000; // restore MC1_Batterystation = 01

            //  Tx_MC1.data[4] = 0b01000001; // Tx_MC1.data[4] = 0b01000001; // MC1_Batterystation = 01, MC1_MotorModeReq = 0,   MC1_VehicleMode = 00001: Torque mode

            Tx_MC1.data[5] = 0b01100000; // MC1_HandbrakeStatus = 1,  MC1_Reg_ChaContactor = MC1_VehiclePRND = 1000 P gear/invalid
            Tx_MC1.NeedUpdate = true;
            while (Tx_MC1.NeedUpdate) ;
        }

        public override void Start()
        {

            Tx_MC1.data[4] &= 0b00011111; // reset flags
            Tx_MC1.data[4] |= 0b01100000; // restore MC1_Batterystation = 01 MC1_MotorModeReq = 1

            //  Tx_MC1.data[4] = 0b01100001; // MC1_Batterystation = 01, MC1_MotorModeReq = 1,   MC1_VehicleMode = 00001: Torque mode

            Tx_MC1.data[5] = 0b00000100; // MC1_HandbrakeStatus = 0,  MC1_Reg_ChaContactor = 0001 forward gear
            Tx_MC1.NeedUpdate = true;
        }

        public override void Stop()
        {
            Tx_MC1.data[4] &= 0b00011111; // reset flags
            Tx_MC1.data[4] |= 0b01000000; // restore MC1_Batterystation = 01

            //     Tx_MC1.data[4] = 0b01000001; // MC1_Batterystation = 01, MC1_MotorModeReq = 0,   MC1_VehicleMode = 00001: Torque mode

            Tx_MC1.data[5] = 0b00010000; // MC1_HandbrakeStatus = 0,  MC1_Reg_ChaContactor = 0100 neutral gear
            Tx_MC1.NeedUpdate = true;
        }

        protected override void Encode(ICAN.RxTxCanData data)
        {


            if (data.id == (uint)CAN_IDs.SW_ID)
            {
                if (data.data[1] == 'E')
                {
                    data.data[0] = (byte)' ';
                    _name = $"EVM inverter [ {Encoding.UTF8.GetString(data.data)} ]";
                }
                else
                {
                    _name = $"3in1 inverter [ {(0.1 * data.data[0])} ]";
                }
            }

            if (data.Timeout) 
            {
                base._state = DeviceState.NoConnect;
                return;
            }


            EncodeData(data);
            EncodeFaults(data);
        }

        public override string Name => _name;

        

        void InitDeviceDescription()
        {
            List<ShanghaiDeviceParameter> tmp = new();

            try
            {
                string fileContents = utils.utils.ReadJsonFromResources(Resources.shanghai_faults);
                FaultsList = JsonConvert.DeserializeObject<List<ShanghaiDeviceFault>>(fileContents);

                fileContents = utils.utils.ReadJsonFromResources(Resources.shanghai_description);
                tmp = JsonConvert.DeserializeObject<List<ShanghaiDeviceParameter>>(fileContents);

                base.DeviceDescription.Clear();
                foreach (var item in tmp)
                {
                    item.device = this;
                    base.DeviceDescription.Add(item);
                }


                // speed monitor Need to be in kRPM!
                _outSpeed = new ShanghaiDeviceParameter(this) {
                    max = 12,
                    min = -12,
                    sname = ((ShanghaiDeviceParameter)base.DeviceDescription[2]).Name,
                    CANID = ((ShanghaiDeviceParameter)base.DeviceDescription[2]).ID
                };

                base.DeviceDescription[2].Value.Subscribe((_) => {
                    
                    ((ShanghaiDeviceParameter)_outSpeed).Val.OnNext(_);
                });

                // current monitor 
                _outCurrent = new ShanghaiDeviceParameter(this)
                {
                    max = 500,
                    min =   0,
                    sname = ((ShanghaiDeviceParameter)base.DeviceDescription[6]).Name,
                    CANID = ((ShanghaiDeviceParameter)base.DeviceDescription[6]).ID
                };

                base.DeviceDescription[6].Value.Subscribe((_) => {

                    ((ShanghaiDeviceParameter)_outCurrent).Val.OnNext(_);
                });

                // Torque monitor 
                _outTorque = new ShanghaiDeviceParameter(this)
                {
                    max = 2,
                    min = -2.0,
                    sname = ((ShanghaiDeviceParameter)base.DeviceDescription[0]).Name,
                    CANID = ((ShanghaiDeviceParameter)base.DeviceDescription[0]).ID
                };

                base.DeviceDescription[0].Value.Subscribe((_) => {

                    ((ShanghaiDeviceParameter)_outCurrent).Val.OnNext(_ / base.DeviceDescription[0].Max);
                });

                // Voltage monitor 
                _outVoltage = base.DeviceDescription[5];

                // Speed control
                _inSpeed = base.DeviceDescription[16];
               
                // Torque control
                _inTorque = base.DeviceDescription[15];

            }
            catch (JsonReaderException e)
            {
                this.Log().Error(e.Message);
            }
        }

        internal class ShanghaiDeviceFault : IDeviceFault, IEnableLogger
        {
            public string ID => code.ToString();

            public string Name => name;

            [JsonProperty("code")]
            internal uint code;

            [JsonProperty("name")]
            internal string name;
        }

        internal class ShanghaiDeviceParameter : IDeviceParameter, IEnableLogger
        {
            internal ShanghaiDevice device;
            public ShanghaiDeviceParameter(ShanghaiDevice device)
            {
                Val.Subscribe(x => _value = x);
                this.device = device;
            }

            public void writeValue(double value)
            {

                if (ID == "0xCF10AD0")
                {
                    device.Tx_MC1 = Convert(device.Tx_MC1, value);
                    this.Log().Info($"0xCF10AD0 offset {offset} len {len} <- {value}");

                }
                else if (ID == "0xC50A4D0")
                {
                    device.Tx_MC2 = Convert(device.Tx_MC2, value);
                    this.Log().Info($"0xCF10AD0 offset {offset} len {len} <- {value}");

                }
                else if (ID == "0xCF12AD0")
                {
                    device.Tx_MC3 = Convert(device.Tx_MC3, value);
                    this.Log().Info($"0xCF10AD0 offset  {offset}  len  {len}  <-  {value}");

                }
                else {
                    this.Log().Warn($"Unknown ID {ID} ");
                }

            }

            private ICAN.RxTxCanData Convert(ICAN.RxTxCanData data, double value) {

                ICAN.RxTxCanData ret_val = data;
                Int32[] val_int;
                if (Options != null && Options.Count > 0)
                {
                    val_int = new Int32[] { (Int32) value };
                }
                else
                {
                    val_int = new Int32[] { (Int32)((value + (double)valoffset) / scale) }; 
                }

                BitArray dst = new BitArray(data.data);
                BitArray src = new BitArray(val_int);
                utils.utils.CopySliceTo(dst, offset, src, 0, len);
                dst.CopyTo(ret_val.data, 0);
                ret_val.NeedUpdate = true;
                return ret_val;    
            }

            internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);

            internal double _value = 0;

            public IObservable<double> Value { get => Val; }

            public string ID { get => CANID; }

            public string Name { get => sname; }

            public string Unit { get => unit; }

            public List<List<string>> Options { get => options; }

            public bool IsReadWrite { get => RW; }

            public double Min { get => min; }

            public double Max { get => max; }

            public string Type { get => null; }

            [JsonProperty("CANID")]
            internal string CANID;

            [JsonProperty("sname")]
            internal string sname { get; set; }

            [JsonProperty("unit")]
            internal string unit { get; set; }

            [JsonProperty("options")]
            internal List<List<string>> options { get; set; }

            [JsonProperty("RW")]
            internal bool RW { get; set; }

            [JsonProperty("len")]
            internal int len;

            [JsonProperty("offset")]
            internal int offset;

            [JsonProperty("valoffset")]
            internal int valoffset;

            [JsonProperty("scale")]
            internal float scale { get; set; }

            [JsonProperty("type")]
            internal string type { get; set; }

            [JsonProperty("def")]
            internal int def { get; set; }

            [JsonProperty("min")]
            internal double min { get; set; }

            [JsonProperty("max")]
            internal double max { get; set; }

            public double ValueNow => _value;
        }
    }
}
