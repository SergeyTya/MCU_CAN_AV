using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Splat;
using System.IO;
using System.Threading;

namespace SerialToSocket
{
    public interface ISerialToSocket {

        void Start(string serial_name, int serial_speed, string socket_name, int socket_port);
        void Stop();

    }

    public class SerialToSocket: ISerialToSocket, IEnableLogger
    {
        readonly string serial_name = String.Empty;
        readonly int serial_speed;

        readonly string socket_name = String.Empty;
        readonly int socket_port;

        List<TcpClient> Clients = new();

        SerialPort? serial;
        ISerialTransport serialTransport;
        TcpListener SerialToSocketServer;

        public void Start(string serial_name, int serial_speed, string socket_name, int socket_port )
        {

            this.Log().Warn($"Starting new serial to socket server ");
           
            serial = new SerialPort(portName: serial_name, baudRate: serial_speed, parity: Parity.None, dataBits: 8);

            serial.ReadBufferSize = 5024;
            serial.WriteBufferSize = 5024;

            try {
                serial.Open();
            }
            catch ( Exception e )
            {
                throw new SerialToSocketException(e.Message);
            }

            this.Log().Warn($"Serial {serial_name}:{serial_speed} connected");

            /********** START new TCP server */
          
            serialTransport = new SerialTransport(serial);

            IPAddress ipa = IPAddress.Parse("127.0.0.1");
            if (socket_name != "localhost") {
                ipa = IPAddress.Parse(socket_name);
            }

            SerialToSocketServer = new TcpListener(ipa, socket_port );
            SerialToSocketServer.Start();

            this.Log().Warn($"Socket {socket_name}:{socket_port} started");

            Task.Factory.StartNew(() => {
                while (true)
                {
                    try
                    {
                        var client = SerialToSocketServer.AcceptTcpClient();
                        client.NoDelay = true;
                        this.Log().Info($"New connection from {client.Client.RemoteEndPoint}");
                        Task.Run(async () => await Accept(client));
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        this.Log().Error(e.Message);
                        return;
                    }
                }
            });
            /********************************/

        }

        int cnt = 0;
        async Task Accept(TcpClient client)
        {
            await Task.Yield();
            var responseData = new byte[2048];
            var stream = client.GetStream();
            try
            {
                while (true)
                {
                    //CancellationTokenSource cts1 = new CancellationTokenSource(100000);
                    //CancellationToken token1 = cts1.Token;
                    //int bytesRead = await stream.ReadAsync(responseData, token1);

                    int bytesRead = await stream.ReadAsync(responseData);

                    if (bytesRead == 0) throw  new Exception("Connection closed");

                    byte[] TCP_REQ_FRAME = new byte[bytesRead];
                    Array.Copy(responseData, TCP_REQ_FRAME, bytesRead);

                 //   this.Log().Info($"Read from socket: {BitConverter.ToString(TCP_REQ_FRAME)}");

                    byte[]? RTU_REQ_FARME = RTUtoTCP.ConvertToRTU(TCP_REQ_FRAME);

                    if (RTU_REQ_FARME != null)
                    {
                        int id = cnt++;

                        CancellationTokenSource cts = new CancellationTokenSource(500);
                        CancellationToken token = cts.Token;
                        byte[]? RTU_RES_FARME = await serialTransport.SerialWrite(id, RTU_REQ_FARME, token);

                        if (token.IsCancellationRequested)
                        {
                            string str = $"ID:{id} Error: Serial await timeout 0";
                            //  closeSocket(socket);
                            return;
                        }

                        if (RTU_RES_FARME != null)
                        {
                            byte[]? TCP_RES_FARME = RTUtoTCP.ConvertToTCP(RTU_RES_FARME);

                            if (TCP_RES_FARME != null)
                            {
                                TCP_RES_FARME[0] = TCP_REQ_FRAME[0];
                                TCP_RES_FARME[1] = TCP_REQ_FRAME[1];
                      //          this.Log().Info($"ID:{id} Write to socket: {BitConverter.ToString(TCP_RES_FARME)}");
                                await stream.WriteAsync(TCP_RES_FARME);
                            }
                            else
                            {
                                string str = $"ID:{id} Error: Serial CRC Error";
                                var mes = Encoding.ASCII.GetBytes(str);
                                await stream.WriteAsync(mes);
                                this.Log().Error(str);
                            }
                        }
                        else
                        {
                            string str = $"ID:{id} Error: Serial await timeout";
                            var mes = Encoding.ASCII.GetBytes(str);
                            await stream.WriteAsync(mes);
                            this.Log().Error(str);
                        }
                    }
                    else
                    {
                        var mes = Encoding.ASCII.GetBytes("Error: TCP frame error");
                        await stream.WriteAsync(mes);
                        this.Log().Error(mes);
                    }


                }
            }
            catch (Exception ex)
            {
                stream.Close();
                client.Close();
                this.Log().Error($"Error: {ex.Message}");
            }
        }

        public void Stop()
        {
            serial?.Close();
            SerialToSocketServer?.Stop();
        }

    }



    public class SerialToSocketException : Exception
    {
        public SerialToSocketException(string message) : base(message) { }
    }

}
