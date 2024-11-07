using AsyncSocketTest;
using Avalonia.Threading;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Can.ModbusTCP;
using MCU_CAN_AV.Properties;
using MCU_CAN_AV.utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;



/***
 * 
 *       ***ViewModel***  | ********************************************** IDevice ************************************************  |   *************************************** ICAN **************************************** 
 *   
 *   Tx  IDevice->Description  -> IDeviceParameter.writeValue -> this.txval.OnNext() -> double_to_can() -> Base.TransmitToHardware() -> ICAN.Transmit() -> WriteHoldingsAsync()
 *     
 *   Rx  IDevice->Description  <- IDeviceParameter.Value                  <- this.IDeviceParametr.RxVal.OnNext()    <- base.Encode() <- ICAN.RxObservable <- RxUpdater.onNext() <- base.post() <- ReadHRsAsync() <- ICAN.Recieve() <-TimerCallBack
 *
 */

namespace MCU_CAN_AV.Devices.EVM_DIAG
{
    internal class EVMModbusDevice : BaseDevice, IEnableLogger
    {
        static List<EVMModbusTCPDeviceFault> FaultsList = new();
        static Subject<ICAN.RxTxCanData> TxObservable = new();
        ICAN CAN_Instance;

        public override void Close_instance()
        {

            FaultsList.Clear();
            DeviceDescription.Clear();
            base._Init_stage = true;
        }

        string _name = "no name";
        int _err_cnt = 0;
        IDisposable? TxDisposable;

        public EVMModbusDevice(ICAN CAN):base(CAN)
        {
            this.Log().Info($"New {nameof(EVMModbusDevice)} connection ");
            _err_cnt = 0;
            Init(CANInitStruct);
            CAN_Instance = CAN;

        }

        void Init(ICAN.CANInitStruct InitStruct)
        {

            Task.Run(async () => {

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();


                while ((IModbusTransport)CAN_Instance == null) {
                    await Task.Delay(1);
                }

                while (CAN_Instance.isOpen == false)
                {
                    await Task.Delay(1);
                }

                this.Log().Info($"elapsed { stopwatch.ElapsedMilliseconds} ms");

                stopwatch.Restart();

                var ret_val = await ((IModbusTransport)CAN_Instance).getDevId().ConfigureAwait(false);

                if (ret_val.IndexOf("Time out") != -1) {
                    throw new Exception("Timeout");
                }


                Task.Run(async () =>
                {
                    var res = await ((IModbusTransport)CAN_Instance).ReadRegistersInfoAsync().ConfigureAwait(false);
                    return res;
                }).ToObservable().Take(1).Subscribe(
                    (_) =>
                    {
                        EncodeDeviceDescription(_);
                        Dispatcher.UIThread.Post(() =>
                        {
                            
                            // Need to be in kRPM!
                            ((EVMModbusTCPDeviceParametr)base.DeviceDescription[6])._Min = -9;
                            ((EVMModbusTCPDeviceParametr)base.DeviceDescription[6])._Max =  9;
                            _outSpeed = base.DeviceDescription[6];


                            ((EVMModbusTCPDeviceParametr) base.DeviceDescription[2])._Min = 0;
                            ((EVMModbusTCPDeviceParametr) base.DeviceDescription[2])._Max = 300;
                            _outCurrent  = base.DeviceDescription[2];

                            ((EVMModbusTCPDeviceParametr)base.DeviceDescription[24])._Min = -300.0;
                            ((EVMModbusTCPDeviceParametr)base.DeviceDescription[24])._Max = 300.0;
                            _outTorque   = base.DeviceDescription[24];

                            ((EVMModbusTCPDeviceParametr)DeviceDescription[4])._Min = 0;
                            ((EVMModbusTCPDeviceParametr)DeviceDescription[4])._Max = 1.0;
                            _outVoltage = base.DeviceDescription[4];

                            ((EVMModbusTCPDeviceParametr)base.DeviceDescription[25])._Min = -9000;
                            ((EVMModbusTCPDeviceParametr)base.DeviceDescription[25])._Max = 9000;
                            _inSpeed     = base.DeviceDescription[25];

                            _inTorque = new EVMModbusTCPDeviceParametr(DeviceDescription[26].ID, DeviceDescription[26].Name, true, HoldingType.HTYPE_FLOAT)
                            {
                                _Max = 300.0,
                                _Min = -300.0
                            };
                            ((EVMModbusTCPDeviceParametr)_inTorque).TxVal.Subscribe((_) => {

                                double ref_val = _ / ((EVMModbusTCPDeviceParametr)DeviceDescription[36]).ValueNow;
                                ((EVMModbusTCPDeviceParametr)DeviceDescription[26]).TxVal.OnNext(ref_val);

                            });

                        });

                    },
                    exception => {
                        this.Log().Error(exception.Message);

                    },
                    () => {
                        stopwatch.Stop();
                        this.Log().Info($"Elapsed {stopwatch.ElapsedMilliseconds} ms");
                        Dispatcher.UIThread.Post(() =>
                        {
                            base._Init_stage = false;
                        });
                    }
                );

                return ret_val;

            }).ToObservable().Take(1).Subscribe(
               (value) => { 
                   _name = value.Trim('\0'); 
               },
               exeption => { this.Log().Error(exeption.Message); base._Init_stage = true; },
               () => { }
            );

            try
            {
                string fileContents = MCU_CAN_AV.utils.utils.ReadJsonFromResources(Resources.EVM_faults);
                FaultsList = JsonConvert.DeserializeObject<List<EVMModbusTCPDeviceFault>>(fileContents);

            }
            catch (JsonReaderException e)
            {
                this.Log().Error(e.Message);
            }

        }
        protected override void Encode(ICAN.RxTxCanData data)
        {
            if (data.Timeout == true) {

                base._Connection_errors_cnt = _err_cnt++;
                base._state = DeviceState.NoConnect;
                return;
            }

            if (data.id == 1 || data.id == 2 || data.id == 3 || data.id == 4)
            {

                BitArray bits = new BitArray(data.data);

                for (int i = 0; i < 16; i++)
                {
                    if (bits[i] == true)
                    {
                        var res = FaultsList
                            .Where(x => x.adr == (i + (data.id - 1) * 16));
                           // .Where(x => base.DeviceFaults.IndexOf((IDeviceFault)x) == -1);

                        foreach (var x in res) { base._DeviceFault = x ; }
                        if (data.id == 1) {
                            if (i == 0)
                            {
                                base._state = DeviceState.Run;
                            }
                            if (i == 1)
                            {
                                base._state = DeviceState.Ready;
                            }
                            if (i == 2)
                            {
                                base._state = DeviceState.Fault;
                            }
                        }
                    }
                }
                return;
            }

            foreach (EVMModbusTCPDeviceParametr item in base.DeviceDescription)
            {
                foreach (var id in item._ids)
                {
                    if (data.id == id)
                    {

                        UInt16[] tmp = { 0 };

                        Buffer.BlockCopy(data.data, 0, tmp, 0, 2);

                        item.data[item._ids.IndexOf(id)] = tmp[0];

                        if (item._ids.Last() == id)
                        {

                            UInt32[] tmp2 = { 0 };
                            Buffer.BlockCopy(item.data.ToArray(), 0, tmp2, 0, 4);

                            UInt32 val0 = tmp2[0];
                            double ret_val = 0;
                            switch (item._Type)
                            {
                                case HoldingType.HTYPE_INT16:
                                    ret_val = (Int16)val0;
                                    break;
                                case HoldingType.HTYPE_UINT16:
                                    ret_val = (UInt16)val0;
                                    break;
                                case HoldingType.HTYPE_INT32:
                                    ret_val = (Int32)val0;
                                    break;
                                case HoldingType.HTYPE_UINT32:
                                    ret_val = (UInt32)val0;
                                    break;
                                case HoldingType.HTYPE_FLOAT:
                                    ret_val = (float)BitConverter.ToSingle(BitConverter.GetBytes(val0), 0);
                                    break;
                                default:
                                case HoldingType.NONE:
                                    break;
                            }

                            if (item._val != ret_val)
                            {
                                item.RxVal.OnNext(ret_val);
                                item._val = ret_val;
                            }
                        }
                    }
                }
            }
        }

        public override string Name { get{ return _name; } }

      
        public override void Reset()
        {
           // base.DeviceFaults.Clear();
            TransmitToHardware(new ICAN.RxTxCanData(0, new byte[] { 4, 0 }));
            this.Log().Info("Reset command sent");

        }

        public override void Start()
        {
            TransmitToHardware(new ICAN.RxTxCanData(0, new byte[] { 1, 0 }));
            this.Log().Info("Start command sent");
        }

        public override void Stop()
        {
            TransmitToHardware(new ICAN.RxTxCanData(0, new byte[] { 2, 0 }));
            this.Log().Info("Stop command sent");
        }

        void EncodeDeviceDescription(List<byte[]> data)
        {
            // skip firs 3 items
            for (uint i = 0; i < data.Count; i++)
            {
                if (i == 1 || i == 2) continue;

                var RXbuf = data[(int)i];

                if (RXbuf[7] != 27)
                {
                    //  IDevice.Log($"Holding info response error FC = {RXbuf[7]}");
                    throw new Exception("Holding encode error");
                }
                UInt32 hl_adr = BitConverter.ToUInt16(RXbuf.ToArray(), 9);
                UInt32 adr = BitConverter.ToUInt32(RXbuf.ToArray(), 10);
                //Debug.WriteLine(String.Format("HR location = {0}", adr));
                var type = RXbuf[14];
                //  ICAN.LogUpdater.OnNext(String.Format("HR type = {0}", type));
                var index = RXbuf[15];
                //Debug.WriteLine(String.Format("HR index = {0}", index));
                var isRO = RXbuf[16];

                string info = Encoding.UTF8.GetString(RXbuf.ToList().GetRange(17, RXbuf[5] - 11).ToArray());

                var tmp = new EVMModbusTCPDeviceParametr(
                            _ID: i.ToString(),
                            _Name: info,
                            _IsReadWrite: isRO == 0,
                            _Type: (HoldingType)type
                        );

                if (
                     (HoldingType)type != HoldingType.HTYPE_INT16
                  && (HoldingType)type != HoldingType.HTYPE_UINT16
                 )
                {
                    tmp._ids = new uint[] { i, ++i };
                }
                else
                {
                    tmp._ids = new uint[] { i };
                }

                tmp.valDisposable = tmp.TxVal.Subscribe((value) =>
                {
                    if (tmp.IsReadWrite != true) return; ;
                    if (tmp._val != value)
                    {
                        double_to_can(value, tmp._Type, tmp._ids);
                    }
                });

                this.Log().Info($"<- ID [{tmp.ID}] Info [ {tmp.Name}]");
                base.DeviceDescription.Add(tmp);
            }
        }

        void double_to_can(double value, HoldingType _Type, uint[] _ids) {

            byte[] bval = GetByteFromString(value.ToString(), _Type);
            if (bval == null) return;
            int count = 0;
            int id = 0;
            var buf = bval.ToList()
                .GroupBy(_ => count++ / 2)
                .Select(v => new ICAN.RxTxCanData((_ids[id++]), v.ToArray()));

            foreach (var item in buf)
            {
                TransmitToHardware(item);
            };
        }

        public class EVMModbusTCPDeviceParametr : IDeviceParameter, IEnableLogger, IDisposable
        {
            string _ID;
            string _Name;
            internal string _Unit;
            internal double _Min;
            internal double _Max;
            bool _IsReadWrite;


            internal HoldingType _Type;
            internal UInt16[] data = new UInt16[4];
            internal uint[] _ids;
            internal double _val;

            internal double _ValueNow = 0;

            public EVMModbusTCPDeviceParametr(
                string _ID, 
                string _Name, 
                bool _IsReadWrite, 
                HoldingType _Type, 
                string _Unit = "",
                double _Min = 0, 
                double _Max = 0
            )
            {
                this._ID = _ID;
                this._Name = _Name;
                this._Unit = _Unit;
                this._Min = _Min;
                this._Max = _Max;
                this._IsReadWrite = _IsReadWrite;
                this._Type = _Type;

                Value.Subscribe(_ => _ValueNow = _);
            }

            // need to tranfer from CAN to register
            public IObservable<double> Value { get => RxVal; }
            internal BehaviorSubject<double> RxVal = new BehaviorSubject<double>(0);
            public IDisposable valDisposable;
          
            //need to tranfer from register to CAN
            internal Subject<double> TxVal = new Subject<double>();

            public string ID => _ID;

            public string Name => _Name;

            public string Unit => _Unit;

            public double Min => _Min;

            public double Max => _Max;

            public string Type => throw new NotImplementedException();

            public bool IsReadWrite => _IsReadWrite;

            public List<List<string>>? Options => null;

            public double ValueNow => _ValueNow;

            void IDeviceParameter.writeValue(double value)
            {
                this.Log().Info($"{ID} <- {value} ");
                TxVal.OnNext(value);
            }

            public void Dispose()
            {
                valDisposable?.Dispose();
                RxVal?.Dispose();
                TxVal?.Dispose();
            }
        }

        public static byte[] GetByteFromString(string str, HoldingType type)
        {

            byte[] ret_val = null;
            switch (type)
            {
                case HoldingType.HTYPE_INT16:
                    if (Int16.TryParse(str, out Int16 res_val) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val);
                    break;
                case HoldingType.HTYPE_UINT16:
                    if (UInt16.TryParse(str, out UInt16 res_val1) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val1);
                    break;
                case HoldingType.HTYPE_INT32:
                    if (Int32.TryParse(str, out Int32 res_val2) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val2);
                    break;
                case HoldingType.HTYPE_UINT32:
                    if (UInt32.TryParse(str, out UInt32 res_val3) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val3);
                    break;
                case HoldingType.HTYPE_FLOAT:
                    if (Single.TryParse(str, out float res_val4) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val4);
                    break;
                default:
                case HoldingType.NONE:
                    break;
            }
            return ret_val;
        }

        internal class EVMModbusTCPDeviceFault : IDeviceFault
        {
            public string ID => adr.ToString();

            public string Name => name;

            [JsonProperty("adr")]
            internal uint adr;

            [JsonProperty("name")]
            internal string name;
        }
    }

    public enum HoldingType : byte
    {
        HTYPE_UINT16,
        HTYPE_INT16,
        HTYPE_UINT32,
        HTYPE_INT32,
        HTYPE_FLOAT,
        NONE = 0xff
    }

}
