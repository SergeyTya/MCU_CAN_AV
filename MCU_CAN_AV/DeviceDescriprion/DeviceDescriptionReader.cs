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

namespace MCU_CAN_AV.DeviceDescriprion
{
    internal class DeviceDescriptionReader
    {

        private static string read_jsonres(byte[] res)
        {
            MemoryStream MS = new MemoryStream(res);
            StreamReader sr = new StreamReader(MS);
            string fileContents = sr.ReadToEnd();
            sr.Close();
            return fileContents;
        }


        public static List<DeviceDescriptionReader> ShanghaiDevice = new List<DeviceDescriptionReader>();

        [JsonProperty("CANID")]
        internal string CANID;

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
        internal List<string> options { get; set; }

        [JsonProperty("type")]
        internal string type { get; set; }

        [JsonProperty("def")]
        internal int def { get; set; }

        [JsonProperty("min")]
        internal int min { get; set; }

        [JsonProperty("max")]
        internal int max { get; set; }

        [JsonProperty("RW")]
        internal bool RW { get; set; }

        public static void Read()
        {
            try
            {
                string fileContents = read_jsonres(Resources.shanghai_description);
                ShanghaiDevice = JsonConvert.DeserializeObject<List<DeviceDescriptionReader>>(fileContents);


            }
            catch (JsonReaderException e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
