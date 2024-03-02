using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Reactive.Subjects;
using System.Reflection.Metadata;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;

namespace SerialToSocket
{
    internal class SocketServer
    {
        private IPEndPoint ip;
        private Socket socket;
        private int max_conn;
        private ManualResetEvent acceptEvent = new ManualResetEvent(false);

        ISerialTransport Serial;

        public SocketServer(string ip, int port, int max_conn, ISerialTransport serial)
        {
            this.ip = new IPEndPoint(IPAddress.Parse(ip), port);
            this.socket = new Socket(this.ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.max_conn = max_conn;
            Serial = serial;
        }

        public void Init()
        {
            this.socket.Bind(this.ip);
            this.socket.Listen(this.max_conn);
            StartListening();
        }

        private void StartListening()
        {
            Console.WriteLine("Server starting...");
            while (true)
            {
                this.socket.BeginAccept(new AsyncCallback(AcceptCallBack), this.socket);
            }
        }

      
        private void AcceptCallBack(IAsyncResult ar)
        {
            Socket? socket = (Socket)ar.AsyncState;
            if (socket == null) return;
            Socket? accept_socket = socket?.EndAccept(ar);
            acceptEvent.Set();

            Console.WriteLine("A new connection. IP:port = " + accept_socket?.RemoteEndPoint?.ToString());


            if (socket == null) throw new ArgumentNullException(nameof(socket));

            ObjectState state = new ObjectState();
            state.socket = accept_socket;

            accept_socket?.BeginReceive(state.buf, 0, ObjectState.buf_sz, 0, new AsyncCallback(RecieveCallBack), state);

        }

        private void RecieveCallBack(IAsyncResult ar)
        {
            
            ObjectState state = (ObjectState) ar.AsyncState;
            Socket socket = state.socket;
            int bytesRead = 0;
            try
            {
                bytesRead = socket.EndReceive(ar);
  
            }
            catch (Exception ex)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return;
            }
           
            if (bytesRead > 0)
            {
                byte[] TCP_REQ_FRAME = new byte[bytesRead];
                Array.Copy(state.buf, TCP_REQ_FRAME, bytesRead);

                Console.WriteLine($"Read from socket: {BitConverter.ToString(TCP_REQ_FRAME)}");

                byte[]? RTU_REQ_FARME = RTUtoTCP.ConvertToRTU(TCP_REQ_FRAME);

                if (RTU_REQ_FARME != null)
                {
                    Task.Run(async () =>
                    {
                        byte[]? RTU_RES_FARME = null;

                        RTU_RES_FARME = await Serial.SerialWrite(RTU_REQ_FARME);

                        return RTU_RES_FARME;

                    }).ToObservable().Take(1).Subscribe((RTU_RES_FARME) => {

                        if (RTU_RES_FARME != null)
                        {
                            byte[]? TCP_RES_FARME = RTUtoTCP.ConvertToTCP(RTU_RES_FARME);

                            if (TCP_RES_FARME != null)
                            {
                                Console.WriteLine($"Write to socket: {BitConverter.ToString(TCP_RES_FARME)}");
                                Send(socket, TCP_RES_FARME);
                            }
                            else 
                            {
                                string str = "Error: Serial CRC Error";
                                var mes = Encoding.ASCII.GetBytes(str);
                                Send(socket, mes);
                                Console.WriteLine(str);
                            }
                        }
                        else {
                            string str = "Error: SerialTimeout";
                            var mes = Encoding.ASCII.GetBytes(str);
                            Send(socket, mes);
                            Console.WriteLine(str);
                        }
                    });
                }
                else
                {
                    var mes = Encoding.ASCII.GetBytes("Error: TCP frame error");
                    Send(socket, mes);
                    Console.WriteLine(mes);
                }
            }
        }

        private void Send(Socket socket, byte[] bytedata)
        {
            socket.BeginSend(bytedata, 0, bytedata.Length, 0 ,new AsyncCallback(SendCallback), socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try {
                Socket socket = (Socket) ar.AsyncState;
                if (socket == null) return;
                int byteSent = socket.EndSend(ar);
                Console.WriteLine($"Sent: {byteSent} to client");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                    
            } catch (Exception e){
                Console.WriteLine(e.ToString());
            }
        }


      
    }


    public class ObjectState
    {
        public Socket? socket;
        public const int buf_sz = 1024;
        public byte[] buf = new byte[buf_sz];
    }

}
