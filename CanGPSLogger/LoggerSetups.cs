using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Peak.Can.Basic;
using Splat;

namespace CanGPSLogger
{
    public class LoggerSetups: IEnableLogger
    {
        [JsonProperty("ComName")]
        public string comName { get; set; }

        [JsonProperty("ComSpeed")]
        public int comSpeed { get; set; }

        [JsonProperty("PCANChannel")]
        public PcanChannel PCANChannel { get; set; }

        [JsonProperty("PCANSpeed")]
        public Bitrate PCANSpeed { get; set; }

        internal LoggerSetups() {
            comName = "com1";
            comSpeed = 4800;
            PCANChannel = PcanChannel.Usb01;
            PCANSpeed = Bitrate.Pcan500;  // 500 - 28, 250 - 284
        }

        public void write()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            File.WriteAllText("logger_setups.json", jsonString);
            this.Log().Error("New setting file created");
        }

        public static LoggerSetups read()
        {
            try
            {
                string jsonString = File.ReadAllText("logger_setups.json", Encoding.Default);
                return JsonConvert.DeserializeObject<LoggerSetups>(jsonString);
            }
            catch (Exception e)
            {
                LoggerSetups inst = new LoggerSetups();
                inst.write();
                
                return inst;
            }
        }

    }
}
