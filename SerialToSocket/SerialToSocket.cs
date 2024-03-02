using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialToSocket
{
    interface ISerialToSocket {

        void Start(string serial_name, int serial_speed, string socket_name, int socket_port);
        void Stop();

    }



    public class SerialToSocket: ISerialToSocket
    {
        readonly string serial_name = String.Empty;
        readonly int serial_speed;

        readonly string socket_name = String.Empty;
        readonly int socket_port;

        SerialPort? serial;
        SocketServer? server;

        public void Start(string serial_name, int serial_speed, string socket_name, int socket_port )
        {
            serial = new SerialPort(portName: serial_name, baudRate: serial_speed, parity: Parity.None, dataBits: 8);
            try {
                serial.Open();
            }
            catch ( Exception e )
            {
                throw new SerialToSocketException(e.Message);
            }

            server = new(socket_name, socket_port, 10, new SerialTransport(serial));
            server.Init();

        }

        public void Stop()
        {
            serial?.Close();
         // server.Close()
        }





    }

    public class SerialToSocketException : Exception
    {
        public SerialToSocketException(string message) : base(message) { }
    }

}
