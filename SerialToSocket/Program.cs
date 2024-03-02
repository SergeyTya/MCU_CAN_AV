// See https://aka.ms/new-console-template for more information
using SerialToSocket;
using System.IO.Ports;
using System.Text;






ISerialToSocket server = new SerialToSocket.SerialToSocket();
server.Start("COM2", 921600, "127.0.0.1", 8888);


