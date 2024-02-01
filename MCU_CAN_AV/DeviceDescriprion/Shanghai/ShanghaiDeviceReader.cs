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


            var tester = new MCU_CAN_AV.Models.tester();

            IDisposable listener = tester.updater.Subscribe(
            (_) =>
            {
         
                //Update MetterFaultTable
                Dispatcher.UIThread.Invoke(() =>
                {
                    //this.Faults.Add("fault" + _.id);
                    //if (this.Faults.Count > 10) this.Faults.Clear();


                    // DeviceDescriptionReader.DeviceDescription[11].Value = _.id;
                   // IDeviceReader.DeviceDescription[5].writeValue(_.id);

                    foreach (var item in IDeviceReader.DeviceDescription)
                    {
                        item.writeValue(_.id);
                    }


                });
            });
        }

        static private void EncodeData(ICAN.RxTxCanData mes) {

            BitArray bits = new BitArray(mes.data);

            foreach (ShanghaiDeviceParameter item in IDeviceReader.DeviceDescription) {
                uint id = 0;

                if (!uint.TryParse(item.ID, out id)) { 
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


            public void writeValue(double value)
            {

                Val.OnNext(value);
            }

            internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);
            public IObservable<double> Value { get => Val; }

            public string ID { get => CANID; }

            public string Name { get => sname; }

            public List<string> Options { get => options; }

            public bool IsReadWrite { get => RW; }

            [JsonProperty("CANID")]
            string CANID;

            [JsonProperty("sname")]
            string sname { get; set; }

            [JsonProperty("options")]
            List<string> options { get; set; }

            [JsonProperty("RW")]
            bool RW { get; set; }

            [JsonProperty("len")]
            public int len;

            [JsonProperty("offset")]
            public int offset;

            [JsonProperty("valoffset")]
            public int valoffset;

            [JsonProperty("scale")]
            public float scale { get; set; }

            [JsonProperty("type")]
            public string type { get; set; }

            [JsonProperty("def")]
            public int def { get; set; }

            [JsonProperty("min")]
            public int min { get; set; }

            [JsonProperty("max")]
            public int max { get; set; }

        }
    }
}
