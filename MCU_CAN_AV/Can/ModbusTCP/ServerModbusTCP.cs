using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AsyncSocketTest
{

    interface IModbusFrame {
        bool isValid { get; }
        byte[] getRXbuf { get; }
        byte[] setRXbuf { set; }
        byte[] getTXbuf { get; }
    }

    internal class BaseFrame : IModbusFrame
    {
        byte[] _TXbuf;
        byte[] _RXbuf;
        bool _valid = false;

        public bool isValid { get { return _valid; } }

        public BaseFrame(byte[] raw_request) {
            _TXbuf = raw_request;
            _valid = false;
        }

        public byte[] setRXbuf {
            set {
                _RXbuf = value;
                _valid = true;
            }
        }

        public byte[] getRXbuf { get { return _RXbuf; } }

        public byte[] getTXbuf { get { return _TXbuf; } }

    }

    public class ServerModbusTCP
    {
        TcpClient tcpClient;
       
        string server;
        int port;

        public int Timeout { set; get; } = 300; // ms
        public bool Connected { get; set; } = false;

        public ServerModbusTCP(string server, int port)
        {
            this.server = server;
            this.port = port;
            connect();
           
        }

        public class ServerModbusTCPException : Exception
        {
            public ServerModbusTCPException(string message) : base(message) { }
            public ServerModbusTCPException(byte[] data) : base(System.Text.Encoding.UTF8.GetString(data).TrimEnd('\0')) { }
        }
    

        private void connect() {
            tcpClient = new TcpClient();
            tcpClient.ReceiveTimeout = Timeout*2;

            try
            {
                tcpClient.Connect(server, port);
                Connected = true;
              //  Debug.WriteLine(String.Format("Connected {0}:{1}", server,port));
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                throw new ServerModbusTCPException(ex.Message);
            }
        }

        public void close() {
            tcpClient.Close();
          //  Debug.WriteLine("Connection closed");
        }

        public async Task<UInt16[]> ReadHoldingsAsync(int adr, int start, int count)
        {
            try {
                return await ReadRegsAsync(adr, start, count, 3);
            }
            catch (ServerModbusTCPException e)
            {
                throw e;
            }
        }

        public async Task<UInt16[]> ReadInputsAsync(int adr, int start, int count)
        {
            try
            {
                return await ReadRegsAsync(adr, start, count, 4);
            }
            catch (ServerModbusTCPException e)
            {
                throw e;
            }
        }

        public async Task<byte[]> SendRawDataAsync(byte[] data) {

            var req = new BaseFrame(data);
            try
            {
                await execute_request(req);
                if (req.isValid)
                {
                    return req.getRXbuf;
                }
                throw new ServerModbusTCPException("Not valid response: SendRawDataAsync");
            }
            catch (ServerModbusTCPException e)
            {
                throw e;
            }
            
            
        }

        //private async Task execute_request(IModbusFrame frame) {
        //    if (!Connected) throw new ServerModbusTCPException("TCP connection failed");

        //    BaseFrame request = (BaseFrame)frame;

        //    var stream = tcpClient.GetStream();
        //    var buf_tx = request.getTXbuf;
        //    var task_write = stream.WriteAsync(buf_tx, 0, buf_tx.Length);
        //    if (await Task.WhenAny(task_write, Task.Delay(Timeout)) != task_write)
        //    {
        //        // timeout logic
        //      //  Debug.WriteLine("Write Timeout");
        //        stream.Close();
        //        close();
        //        connect();
        //        throw new ServerModbusTCPException("Write Timeout");

        //    }
        //    var buf_size = 2056;
        //    var buf_rx = new byte[buf_size];
        //    var task_read = stream.ReadAsync(buf_rx, 0, buf_size);
        //    if (await Task.WhenAny(task_read, Task.Delay(Timeout)) != task_read)
        //    {
        //        // timeout logic
        //     //   Debug.WriteLine("Read Timeout");
        //        stream.Close();
        //        close();
        //        connect();
        //        throw new ServerModbusTCPException("Read Timeout");
        //    }


        //    request.setRXbuf = buf_rx;
        //}


        private async Task execute_request(IModbusFrame frame)
        {
            if (!Connected) throw new ServerModbusTCPException("TCP connection failed");

            BaseFrame request = (BaseFrame)frame;

            var stream = tcpClient.GetStream();
            var buf_tx = request.getTXbuf;

            CancellationTokenSource cts = new CancellationTokenSource(Timeout);
            CancellationToken token = cts.Token;
            try
            {
                await stream.WriteAsync(buf_tx, 0, buf_tx.Length, token);
            }
            catch (Exception) {

                stream.Close();
                close();
                connect();
                throw new ServerModbusTCPException("Write Timeout");
            }

            var buf_size = 2056;
            var buf_rx = new byte[buf_size];

            CancellationTokenSource cts1 = new CancellationTokenSource(Timeout/2);
            CancellationToken token1 = cts1.Token;
            try
            {
               await stream.ReadAsync(buf_rx, 0, buf_size, token1);
            }
            catch (Exception)
            {
                stream.Close();
                close();
                connect();
                throw new ServerModbusTCPException("Read Timeout");
            }

            request.setRXbuf = buf_rx;
        }

        public UInt16[] ConvertFromByte(byte[] data) {
            int index = 0;
            var res = data.GroupBy(x => (index++) / 2).Select(x => BitConverter.ToUInt16(x.Reverse().ToArray(), 0)).ToList();
            return res.ToArray();
        }

        private async Task<UInt16[]> ReadRegsAsync(int adr, int start, int count, int type)
        {

            byte[] buf = new byte[]
            { 0, 0, 0, 0, 0, 6,
                (byte)adr,
                (byte)type,
                (byte)
                (start>>8),
                (byte) (start&0xff),
                (byte)(count >> 8),
                (byte)(count & 0xff)
            };

            var req = new BaseFrame(buf);
            await execute_request(req);
            if (req.isValid)
            {
                // check function code
                if (req.getRXbuf[7] == type)
                {
                    // check data size in modbus RTU frame
                    int pld_sz = req.getRXbuf[8];
                    if (pld_sz == count * 2)
                    {
                        byte[] pld8 = new byte[pld_sz];
                        Array.Copy(req.getRXbuf, 9, pld8, 0, pld_sz);
                        return ConvertFromByte(pld8);
                    }
                }
                throw new ServerModbusTCPException("ModbusTCP: Frame error");
            }
            else
            {
                throw new ServerModbusTCPException("ModbusTCP: Frame not valid");
            }
        }

        public async Task<bool> WriteHoldingsAsync(int adr, int start, UInt16[] data)
        {
            List<byte> buf = new List<byte>
            { 0, 0, 0, 0, 0,
                (byte) (7 + data.Length * 2),
                (byte)adr,
                (byte)0x10,
                (byte)(start>>8),
                (byte) (start&0xff),
                (byte)((data.Length) >> 8),
                (byte)((data.Length) & 0xff),
                (byte)(data.Length*2)
            };
            foreach (var item in data)
            {  // change indian
                var res = BitConverter.GetBytes(item);
                Array.Reverse(res);
                buf.AddRange(res);
            }
            
            var req = new BaseFrame(buf.ToArray());
            await execute_request(req);
            if (req.isValid)
            {
                // check function code
                if (req.getRXbuf[7] == 0x10)
                {
                    return true;
                }
                throw new ServerModbusTCPException(req.getRXbuf);
            }
            throw new ServerModbusTCPException("Not valid response: WriteHoldingsAsync");
        }

        public async Task<UInt16[]> ReadWriteHoldingsAsync(int adr, int read_start,int read_count, int write_start, UInt16[] write_data)
        {
            List<byte> buf = new List<byte>
            { 0, 0, 0, 0, 0,
                (byte) (11 + write_data.Length * 2),
                (byte)adr,
                (byte)0x17,
                (byte)(read_start>>8),
                (byte)(read_start&0xff),
                (byte)(read_count >> 8),
                (byte)(read_count & 0xff),
                (byte)(write_start>>8),
                (byte)(write_start&0xff),
                (byte)(write_data.Length >> 8),
                (byte)(write_data.Length & 0xff),
                (byte)(write_data.Length*2)
            };
            foreach (var item in write_data)
            {  // change indian
                var res = BitConverter.GetBytes(item);
                Array.Reverse(res);
                buf.AddRange(res);
            }

            var req = new BaseFrame(buf.ToArray());
            await execute_request(req);
            if (req.isValid) //TODO create check pld function
            {
                // check function code
                if (req.getRXbuf[7] == 0x17)
                {
                    // check data size in modbus RTU frame
                    int pld_sz = req.getRXbuf[8];
                    if (pld_sz == read_count * 2)
                    {
                        byte[] pld8 = new byte[pld_sz];
                        Array.Copy(req.getRXbuf, 9, pld8, 0, pld_sz);
                        return ConvertFromByte(pld8);
                    }
                }
                throw new ServerModbusTCPException(req.getRXbuf);
            }
            else
            {
                throw new ServerModbusTCPException("Not valid response: ReadWriteHoldingsAsync");
            }
        }

        public async Task<string> GetInfoAsync() {

            var buf = Encoding.UTF8.GetBytes("info");
            var req = new BaseFrame(buf);

            await execute_request(req);

            if(req.isValid)
            {
                string info = String.Format("{0}:{1} {2}", server, port, System.Text.Encoding.UTF8.GetString(req.getRXbuf).Trim('\0'));
                return info;
            }

            return String.Empty;
        }







    }
}
