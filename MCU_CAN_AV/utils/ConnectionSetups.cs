using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.utils
{

    public class ConnectionSetups
    {
        public ConnectionSetups(
            bool aconnect = true,
            bool devSearch = true,
            string port = "COM6",
            int baudRate = 9600,
            int slaveAdr = 1,
            string serverName = "localhost",
            int serverPort = 8888,
            bool attach = false,
            bool runServerConsole = false
        )
        {
            Aconnect = aconnect;
            DevSearch = devSearch;
            ComPortName = port;
            BaudRate = baudRate;
            SlaveAdr = slaveAdr;
            ServerName = serverName;
            ServerPort = serverPort;
            Attach = attach;
            RunServerConsole = runServerConsole;
        }

        [JsonProperty("Autoconnect")]
        public bool Aconnect { get; set; }

        [JsonProperty("DevSearch")]
        public bool DevSearch { get; set; }

        [JsonProperty("ComPortName")]
        public string ComPortName { get; set; }

        [JsonProperty("BaudRate")]
        public int BaudRate { get; set; }

        [JsonProperty("SlaveAdr")]
        public int SlaveAdr { get; set; }

        [JsonProperty("ServerName")]
        public string ServerName { get; set; }

        [JsonProperty("ServerPort")]
        public int ServerPort { get; set; }

        [JsonProperty("AttachToRunningServer")]
        public bool Attach { get; set; }

        [JsonProperty("RunServerConsole")]
        public bool RunServerConsole { get; set; }

        public void write()
        {
            //string jsonString = JsonConvert.SerializeObject(this);
            //File.WriteAllText("connection_setups.json", jsonString);
        }

        public static ConnectionSetups read()
        {
            try
            {
            //    string jsonString = File.ReadAllText("connection_setups.json", Encoding.Default);
            //    return JsonConvert.DeserializeObject<ConnectionSetups>(jsonString);
            }
            catch (Exception e)
            {
                //ConnectionSetups inst = new ConnectionSetups();
                //inst.write();
                //Debug.WriteLine(e);
                //return inst;
            }

            ConnectionSetups inst = new ConnectionSetups();
            inst.write();
            return inst;
        }
    }
}
