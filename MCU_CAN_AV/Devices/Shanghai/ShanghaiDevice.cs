using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Devices.EVM_DIAG;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices.Shanghai
{
    internal class ShanghaiDevice : BaseDevice, IEnableLogger
    {
        static List<ShanghaiDeviceFault> FaultsList = new();

        public ShanghaiDevice(ICAN CAN):base(CAN) {
            this.Log().Info($"New {nameof(ShanghaiDevice)} connection ");
            InitDeviceDescription();
        }


        private void EncodeData(ICAN.RxTxCanData mes) {

            BitArray bits = new BitArray(mes.data);

            foreach (ShanghaiDeviceParameter item in base.DeviceDescription) {
                uint id = 0;
                char[] _trim_hex = new char[] { '0', 'x' };

                if (!uint.TryParse(item.ID.TrimStart(_trim_hex), NumberStyles.HexNumber, null, out id)) { 
                    throw new NotImplementedException();
                };

                if (id == mes.id) {

                    var res = CopySlice(bits, item.offset, item.len);
                    int[] val = new int[1];
                    res.CopyTo(val, 0);

                    if (item.Options != null && item.Options.Count > 0)
                    {
                        item.Val.OnNext( val[0]);
                    }
                    else {
                        double vald = ((double)val[0] * (double)item.scale - (double)item.valoffset);
                        item.Val.OnNext(vald);
                    }
                }
            }
        }

        private void EncodeFaults(ICAN.RxTxCanData mes) {

            if ( (FaultsList.Count > 0) && (mes.id == (uint)0x18F39CD1) ){ 

                foreach (var fault_new in FaultsList)
                {
                    if (fault_new.code == mes.data[2])
                    {
                       
                        //bool new_fault = true;
                        //foreach (var fault_now in base.DeviceFaults)
                        //{

                        //    if (((ShanghaiDeviceFault)fault_now).code == fault_new.code)
                        //    {
                        //        new_fault = false;
                        //        //fault_now.Cells[1].Style.BackColor = Color.LightGray;
                        //        //isActive_error = true;
                        //    }
                        //    else
                        //    {
                        //        // fault_now.Cells[1].Style.BackColor = Color.White;
                        //    }
                        //}
                        //if (new_fault)
                        //{
                        //    base.DeviceFaults.Add(fault_new);
                        //}
                        base._DeviceFault = fault_new;

                    }
                }


            }
        }

        private static BitArray CopySlice(BitArray source, int offset, int length)
        {
            // Urgh: no CopyTo which only copies part of the BitArray
            BitArray ret = new BitArray(length);
            for (int i = 0; i < length; i++)
            {
                ret[i] = source[offset + i];
            }
            return ret;
        }

        public override void Close_instance()
        {
          //  base.DeviceFaults.Clear();
        }

        public override void Reset()
        {
            //base.DeviceFaults.Clear();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        protected override void Encode(ICAN.RxTxCanData data)
        {
            EncodeData(data);
            EncodeFaults(data);
        }

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
                    base.DeviceDescription.Add(item);
                }
            }
            catch (JsonReaderException e)
            {
                throw new NotImplementedException();
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

            public ShanghaiDeviceParameter() {
                Val.Subscribe(x => _value = x);
            }

            public void writeValue(double value)
            {
               
               
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

            public string Type { get => throw new NotImplementedException(); }

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
        }
    }
}
