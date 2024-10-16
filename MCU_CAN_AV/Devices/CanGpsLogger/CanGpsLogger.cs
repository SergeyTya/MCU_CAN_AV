using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCU_CAN_AV.Can;
using Splat;
using System.IO.Ports;
using SerialToSocket;
using System.Threading;
using ScottPlot.AxisLimitCalculators;
using static MCU_CAN_AV.Devices.Shanghai.ShanghaiDevice;
using NmeaParser;
using System.IO;

namespace MCU_CAN_AV.Devices.CanGpsLogger
{
    internal class CanGpsLogger : BaseDevice, IEnableLogger
    {
        SerialPort? serial;
        public override string Name => "CAN GPS Logger";

        Stream stream = new MemoryStream();
        StreamWriter sw;
        NmeaDevice nmea;
        SerialPortDevice dev;

        public CanGpsLogger(ICAN CAN) : base(CAN)
        {
            sw = new StreamWriter(stream);
            nmea = new StreamDevice(stream);

            serial = new SerialPort();
            serial.BaudRate = 4800;
            serial.Parity = Parity.None;
            serial.PortName = "com3";

            try
            {
                //base._Connection_errors_cnt = 0;
                serial.Open();
                this.Log().Info("GPS port opened");
                //serial.DataReceived += SerialRecived;
                //nmea.MessageReceived += Nmea_MessageReceived;
                //nmea.OpenAsync();

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

        public void SerialRecived(object sender , SerialDataReceivedEventArgs e)
        {
           // var line = serial.ReadLine();
            // this.Log().Info($"SerialRecived: {line}");
           // sw.WriteLine(e);
           

        }

        protected override void Encode(ICAN.RxTxCanData data)
        { 

        }


        public override void Close_instance()
        {
            dev.CloseAsync();
        }

        public override void Reset()
        {
          }

        public override void Start()
        {
              }

        public override void Stop()
        {
                  }






    }


}
