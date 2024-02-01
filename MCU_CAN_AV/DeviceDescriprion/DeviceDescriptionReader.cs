using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using MCU_CAN_AV.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DynamicData.Binding;
using ReactiveUI;

namespace MCU_CAN_AV.DeviceDescriprion
{


    public class DeviceDescriptionReader: ViewModels.ViewModelBase
    {
        private static readonly DeviceDescriptionReader instance = new DeviceDescriptionReader();

        private DeviceDescriptionReader()
        {
            Read();
        }

        private static string read_jsonres(byte[] res)
        {
            MemoryStream MS = new MemoryStream(res);
            StreamReader sr = new StreamReader(MS);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            return fileContents;
        }

        public static ObservableCollection <DeviceParameter> DeviceDescription;
        
        public static void Read()
        {
            List < DeviceParameter > deviceParameters = new List < DeviceParameter >();
            try
            {
                string fileContents = read_jsonres(Resources.shanghai_description);
                DeviceDescription = JsonConvert.DeserializeObject<ObservableCollection<DeviceParameter>>(fileContents);

               // DeviceDescription = new ObservableCollection < DeviceParameter >(deviceParameters); 
            }
            catch (JsonReaderException e)
            {
                Debug.WriteLine(e);
            }
        }

        public static List<DeviceParameter>? ReadList()
        {
            List<DeviceParameter> deviceParameters = new List<DeviceParameter>();
            try
            {
                string fileContents = read_jsonres(Resources.shanghai_description);
                deviceParameters = JsonConvert.DeserializeObject<List<DeviceParameter>>(fileContents);

                //DeviceDescription = new ObservableCollection<DeviceParameter>(deviceParameters);
                return deviceParameters;
            }
            catch (JsonReaderException e)
            {
                Debug.WriteLine(e);

                return null;
            }
        }
    }

    public partial class DeviceParameter : ObservableObject
    {
        [JsonProperty("CANID")]
        public string CANID;

        [JsonProperty("len")]
        public int len;

        [JsonProperty("offset")]
        public int offset;

        [JsonProperty("valoffset")]
        public int valoffset;

        [JsonProperty("scale")]
        public float scale { get; set; }

        [JsonProperty("sname")]
        public string sname { get; set; }

        [JsonProperty("options")]
        public List<string> options { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("def")]
        public int def { get; set; }

        [JsonProperty("min")]
        public int min { get; set; }

        [JsonProperty("max")]
        public int max { get; set; }

        [JsonProperty("RW")]
        public bool RW { get; set; }

        [ObservableProperty]
        public double _value;

        internal EventHandler onValueChanged;
        public event EventHandler onValueChangedByUser
        {
            add
            {
                lock (this) { onValueChanged = onValueChanged + value; }
            }
            remove
            {
                lock (this) { onValueChanged = onValueChanged - value; }
            }
        }
    }
}
