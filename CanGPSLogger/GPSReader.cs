using NmeaParser;
using Splat;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanGPSLogger
{
    internal class GPSReader: IEnableLogger, IDisposable
    {
        SerialPort? serial;
        SerialPortDevice dev;

        public GPSReader() {

            serial = new SerialPort();
            serial.BaudRate = 4800;
            serial.Parity = Parity.None;
            serial.PortName = "com4";
            try
            {
             
                serial.Open();
                this.Log().Info("GPS port opened");

                dev = new SerialPortDevice(serial);
                dev.MessageReceived += Nmea_MessageReceived;
                dev.OpenAsync();

            }
            catch (Exception e)
            {
                this.Log().Error(e);
            }
        }

        private void Nmea_MessageReceived(object? sender, NmeaMessageReceivedEventArgs args)
        {

            if (args.Message is NmeaParser.Messages.Rmc rmc)
            {
                this.Log().Info(args.Message);
                this.Log().Info($"location is: {rmc.Longitude} , {rmc.Longitude}");
            }
        }

        public void Dispose() {
            serial?.Close();
            serial?.Dispose();
        }
    }
}
