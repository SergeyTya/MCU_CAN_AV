// See https://aka.ms/new-console-template for more information
using SerialToSocket;
using System.IO.Ports;
using System.Text;
using Splat.Serilog;
using Serilog;
using Splat;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Locator.CurrentMutable.UseSerilogFullLogger();

ISerialToSocket server = new SerialToSocket.SerialToSocket();
server.Start("COM2", 921600, "127.0.0.1", 8888);


