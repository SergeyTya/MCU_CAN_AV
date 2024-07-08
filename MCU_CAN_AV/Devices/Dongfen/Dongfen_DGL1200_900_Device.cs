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
    internal class Dongfen_DGL1200_900 : BaseDevice, IEnableLogger
    {
        private string _name = "Dongfen_DGL1200_900";
        internal List<Dongfen_80KDeviceFault> FaultsList = new();

        System.Timers.Timer RxTimer;

        private enum CAN_IDs
        {
            FAULT_ID = 0x18FF0D70,
            STATE_ID = 0x18FF0C70,
            CONTROL_ID = 0x0C877010
        }

        internal  ICAN.RxTxCanData Tx_CF1 = new(0x0C877010, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }) { NeedUpdate = true }; // 10  ms
      
        Subject<ICAN.RxTxCanData> TxSubject = new Subject<ICAN.RxTxCanData>();
     
        CancellationTokenSource cts = new CancellationTokenSource();
       
        private SemaphoreSlim _semaphore = new(1);
 
        private IDisposable? TxDisposable;
        private IDisposable? Disposable_CF;

        public override string Name { get { return _name; } }



        ///
        public Dongfen_DGL1200_900(ICAN CAN) : base(CAN)
        {
           //_name = "Shanghai 3in1";


            this.Log().Info($"New {nameof(Dongfen_DGL1200_900)} connection ");
            this.Log().Warn($"Can speed new value is {CAN.InitStructure._Baudrate} ");
            this.Log().Warn($"Polling interval new value is  {CAN.InitStructure._PollInterval_ms} ms ");

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

            Disposable_CF = Observable.Interval(TimeSpan.FromMilliseconds(100)).Subscribe((_) =>
            {
                if (!TxSubject.IsDisposed) TxSubject.OnNext(Tx_CF1);
            });
        }


        private void EncodeData(ICAN.RxTxCanData mes)
        {

            BitArray bits = new BitArray(mes.data);

            foreach (Dongfen_80KDeviceParameter item in base.DeviceDescription)
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
            int fault_cnt = 0;
            if ((FaultsList.Count > 0) && (mes.id == 0x18FF0D70))
            {
                BitArray bits = new BitArray(mes.data);

                for (int i = 0; i < 23; i++)
                {

                    if (bits[i])
                    {

                        foreach (var fault_new in FaultsList)
                        {
                            if (fault_new.code == i)
                            {
                                base._DeviceFault = fault_new;
                                fault_cnt++;
                            }
                        }
                    }
                }

                if (fault_cnt == 0)
                {
                    byte state = (byte) (mes.data[0] & (byte)0x0F);
                    if (state == 2)
                    {
                        base._state = DeviceState.Run;
                    } else

                    if (mes.data[0] == 1)
                    {
                        base._state = DeviceState.Ready;
                    }

                    else
                    {
                        base._state = DeviceState.Fault;
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
            Disposable_CF?.Dispose();
        }

        public override void Reset()
        {
            Tx_CF1.data[0] = 0;
            Tx_CF1.NeedUpdate = true;
            while (Tx_CF1.NeedUpdate) ;
        }

        public override void Start()
        {

            Tx_CF1.data[0] = 2;
            Tx_CF1.NeedUpdate = true;
        }

        public override void Stop()
        {
            Tx_CF1.data[0] = 1;
            Tx_CF1.NeedUpdate = true;
        }

        protected override void Encode(ICAN.RxTxCanData data)
        {

            if (data.Timeout) 
            {
                base._state = DeviceState.NoConnect;
                return;
            }


            EncodeData(data);
            EncodeFaults(data);
        }

  

        void InitDeviceDescription()
        {
            List<Dongfen_80KDeviceParameter> tmp = new();

            try
            {
                string fileContents = utils.utils.ReadJsonFromResources(Resources.Dongfen_DGL1200_900_faults);
                FaultsList = JsonConvert.DeserializeObject<List<Dongfen_80KDeviceFault>>(fileContents);

                fileContents = utils.utils.ReadJsonFromResources(Resources.Dongfen_DGL1200_900_description);
                tmp = JsonConvert.DeserializeObject<List<Dongfen_80KDeviceParameter>>(fileContents);

                base.DeviceDescription.Clear();
                foreach (var item in tmp)
                {
                    item.device = this;
                    base.DeviceDescription.Add(item);
                }


                //// speed monitor Need to be in kRPM!
                //_outSpeed = new Dongfen_80KDeviceParameter(this) {
                //    max = 12,
                //    min = -12,
                //    sname = ((ShanghaiDeviceParameter)base.DeviceDescription[2]).Name,
                //    CANID = ((ShanghaiDeviceParameter)base.DeviceDescription[2]).ID
                //};

                //base.DeviceDescription[2].Value.Subscribe((_) => {
                    
                //    ((ShanghaiDeviceParameter)_outSpeed).Val.OnNext(_);
                //});

                //// current monitor 
                //_outCurrent = new ShanghaiDeviceParameter(this)
                //{
                //    max = 500,
                //    min =   0,
                //    sname = ((ShanghaiDeviceParameter)base.DeviceDescription[6]).Name,
                //    CANID = ((ShanghaiDeviceParameter)base.DeviceDescription[6]).ID
                //};

                //base.DeviceDescription[6].Value.Subscribe((_) => {

                //    ((ShanghaiDeviceParameter)_outCurrent).Val.OnNext(_);
                //});

                //// Torque monitor 
                //_outTorque = new ShanghaiDeviceParameter(this)
                //{
                //    max = 300,
                //    min = -300.0,
                //    sname = ((ShanghaiDeviceParameter)base.DeviceDescription[0]).Name,
                //    CANID = ((ShanghaiDeviceParameter)base.DeviceDescription[0]).ID
                //};

                //base.DeviceDescription[0].Value.Subscribe((_) => {

                //    ((ShanghaiDeviceParameter)_outTorque).Val.OnNext(_);
                //});

                //// Voltage monitor 
                //_outVoltage = base.DeviceDescription[5];

                //// Speed control
                //_inSpeed = base.DeviceDescription[16];


                //((ShanghaiDeviceParameter) base.DeviceDescription[15]).max =  300.0;
                //((ShanghaiDeviceParameter) base.DeviceDescription[15]).min = -300.0;
                //// Torque control
                //_inTorque = base.DeviceDescription[15];

            }
            catch (JsonReaderException e)
            {
                this.Log().Error(e.Message);
            }
        }

        internal class Dongfen_80KDeviceFault : IDeviceFault, IEnableLogger
        {
            public string ID => code.ToString();

            public string Name => name;

            [JsonProperty("code")]
            internal uint code;

            [JsonProperty("name")]
            internal string name;
        }

        internal class Dongfen_80KDeviceParameter : IDeviceParameter, IEnableLogger
        {
            internal Dongfen_DGL1200_900 device;
            public Dongfen_80KDeviceParameter(Dongfen_DGL1200_900 device)
            {
                Val.Subscribe(x => _value = x);
                this.device = device;
            }

            public void writeValue(double value)
            {

                if (ID == "0x0C877010")
                {
                    device.Tx_CF1 = Convert(device.Tx_CF1, value);
                    this.Log().Info($"0x0C877010 offset {offset} len {len} <- {value}");

                } else {
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
