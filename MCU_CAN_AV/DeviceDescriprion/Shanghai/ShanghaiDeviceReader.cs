using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace MCU_CAN_AV.DeviceDescriprion.Shanghai
{
    internal class ShanghaiDeviceReader : IDeviceReader
    {
        static public void Init()
        {
            List<ShanghaiDeviceParameter> tmp = new();
            try
            {
                string fileContents = IDeviceReader.ReadJsonFromResources(Resources.shanghai_description);
                tmp = JsonConvert.DeserializeObject<List<ShanghaiDeviceParameter>>(fileContents);

                IDeviceReader.DeviceDescription.Clear();
                foreach (var item in tmp)
                {
                    IDeviceReader.DeviceDescription.Add(item);
                }
            }
            catch (JsonReaderException e)
            {
                throw new NotImplementedException();
            }


            //var tester = new MCU_CAN_AV.Models.tester();

            //IDisposable listener = tester.updater.Subscribe(
            //(_) =>
            //{

            //    //Update MetterFaultTable
            //    Dispatcher.UIThread.Invoke(() =>
            //    {
            //        //this.Faults.Add("fault" + _.id);
            //        //if (this.Faults.Count > 10) this.Faults.Clear();

            //        foreach (ShanghaiDeviceParameter item in IDeviceReader.DeviceDescription)
            //        {

            //            if (item._value != _.id)
            //            {
            //                item.writeValue(_.id); // we weel post only new values
            //            }
            //            else {

            //                //Debug.WriteLine("Equals!");
            //            }
            //        }
            //    });
            //});

            var CAN = ICAN.Create(
                      new ICAN.CANInitStruct(
                   DevId: 0, CANId: 0, Baudrate: 500, RcvCode: 0, Mask: 0xffffffff, Interval: 100
              ));


            IDisposable listener = CAN.Start().Subscribe(
            (_) =>
            {
                Dispatcher.UIThread.Invoke(() =>
                    {
                        EncodeData(_);
                    });
            });

        }

        static private void EncodeData(ICAN.RxTxCanData mes) {

            BitArray bits = new BitArray(mes.data);

            foreach (ShanghaiDeviceParameter item in IDeviceReader.DeviceDescription) {
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

        internal class ShanghaiDeviceParameter : IDeviceParameter
        {

            public ShanghaiDeviceParameter() {
                Val.Subscribe(x => _value = x);
            }

            public void writeValue(double value)
            {

                Val.OnNext(value);
            }

            internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);

            internal double _value = 0;
            public IObservable<double> Value { get => Val; }

            public string ID { get => CANID; }

            public string Name { get => sname; }

            public string Unit { get => unit; }

            public List<string> Options { get => options; }

            public bool IsReadWrite { get => RW; }

            public string Min { get => throw new NotImplementedException(); }

            public string Max { get => throw new NotImplementedException(); }

            public string Type { get => throw new NotImplementedException(); }

            [JsonProperty("CANID")]
            internal string CANID;

            [JsonProperty("sname")]
            internal string sname { get; set; }

            [JsonProperty("unit")]
            internal string unit { get; set; }

            [JsonProperty("options")]
            internal List<string> options { get; set; }

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
            internal int min { get; set; }

            [JsonProperty("max")]
            internal int max { get; set; }
        }
    }
}
