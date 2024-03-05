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
using Splat;

namespace SerialToSocket
{
    internal class SocketServer : IEnableLogger
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
            this.Log().Info("Server starting...");
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
            if (accept_socket == null) return;
            acceptEvent.Set();

            this.Log().Info("A new connection. IP:port = " + accept_socket?.RemoteEndPoint?.ToString());


            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }


            Receive(accept_socket);
        }

        private async void  Receive(Socket accept_socket)
        {
            this.Log().Info("Await new connection");
            ObjectState state = new ObjectState();
            state.socket = accept_socket;
            accept_socket?.BeginReceive(state.buf, 0, ObjectState.buf_sz, 0, new AsyncCallback(RecieveCallBack), state);
        }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        int cnt = 0;


        private async void RecieveCallBack(IAsyncResult ar)
        {


            ObjectState state = (ObjectState)ar.AsyncState;
            Socket socket = state.socket;
            int bytesRead = 0;
            try
            {
                bytesRead = socket.EndReceive(ar);

            }
            catch (Exception ex)
            {
                this.Log().Error(ex.Message);
                closeSocket(socket);
                return;
            }

            if (bytesRead > 0)
            {
                byte[] TCP_REQ_FRAME = new byte[bytesRead];
                Array.Copy(state.buf, TCP_REQ_FRAME, bytesRead);

                this.Log().Info($"Read from socket: {BitConverter.ToString(TCP_REQ_FRAME)}");

                byte[]? RTU_REQ_FARME = RTUtoTCP.ConvertToRTU(TCP_REQ_FRAME);

                if (RTU_REQ_FARME != null)
                {
                    int id = cnt++;

                    CancellationTokenSource cts = new CancellationTokenSource(100);
                    CancellationToken token = cts.Token;
                    byte[]? RTU_RES_FARME = await Serial.SerialWrite(id, RTU_REQ_FARME, token);

                    if(token.IsCancellationRequested) {
                        string str = $"ID:{id} Error: Serial await timeout 0";
                        closeSocket(socket);
                       return; 
                    }

                    if (RTU_RES_FARME != null)
                    {
                        byte[]? TCP_RES_FARME = RTUtoTCP.ConvertToTCP(RTU_RES_FARME);

                        if (TCP_RES_FARME != null)
                        {
                            TCP_RES_FARME[0] = TCP_REQ_FRAME[0];
                            TCP_RES_FARME[1] = TCP_REQ_FRAME[1];
                            this.Log().Info($"ID:{id} Write to socket: {BitConverter.ToString(TCP_RES_FARME)}");
                            Send(socket, TCP_RES_FARME);
                        }
                        else
                        {
                            string str = $"ID:{id} Error: Serial CRC Error";
                            var mes = Encoding.ASCII.GetBytes(str);
                           // Send(socket, mes);
                            this.Log().Error(str);
                            closeSocket(socket);
                        }
                    }
                    else
                    {
                        string str = $"ID:{id} Error: Serial await timeout";
                        var mes = Encoding.ASCII.GetBytes(str);
                    //  Send(socket, mes);
                    //    this.Log().Error(str);
                        closeSocket(socket);
                        return;
                    }

                }
                else
                {
                    var mes = Encoding.ASCII.GetBytes("Error: TCP frame error");
                 //   Send(socket, mes);
                    this.Log().Error(mes);
                    closeSocket(socket);
                }
            }
        }

        private void Send(Socket socket, byte[] bytedata)
        {
            socket.BeginSend(bytedata, 0, bytedata.Length, 0, new AsyncCallback(SendCallback), socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                if (socket == null) return;
                int byteSent = socket.EndSend(ar);
                this.Log().Info($"Sent: {byteSent} to client");
                //socket.Shutdown(SocketShutdown.Both);
                //socket.Close(); 
               Task.Run(() => Receive(socket));
            } 
            catch (Exception e)
            {
                Socket socket = (Socket)ar.AsyncState;
                closeSocket(socket);
                this.Log().Error(e.ToString());
            }
        }

        private void closeSocket(Socket? socket) {

            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
        }



    }


    public class ObjectState
    {
        public Socket? socket;
        public const int buf_sz = 1024;
        public byte[] buf = new byte[buf_sz];
    }

}
