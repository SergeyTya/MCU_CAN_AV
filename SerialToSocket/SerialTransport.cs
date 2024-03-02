using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SerialToSocket
{
    internal interface ISerialTransport
    {
        Task<byte[]?> SerialWrite(byte[] data);   
    }

    public class SerialTransport : ISerialTransport
    {
        SerialPort _serial;
        Subject<byte[]> RxSubject = new();
        public SerialTransport(SerialPort serial) {
            _serial = serial;
            _serial.DataReceived += (_, __) => {

                SerialPort spL = (SerialPort)_;
                byte[] buff = new byte[spL.BytesToRead];
                spL.Read(buff,0, spL.BytesToRead);
                RxSubject.OnNext(buff);

                Console.WriteLine("Read from serial: " + BitConverter.ToString(buff));
            };
        }



        public async Task<byte[]?> SerialWrite(byte[] data)
        {
            byte[]? res = null;

            if (_serial == null) return null;
            if (!_serial.IsOpen) return null;

            _serial.Write(data, 0, data.Length);

            Console.WriteLine("Write to serial: " + BitConverter.ToString(data));

            CancellationToken token = new();
            var t1 = Task.Run(async () => { 
                var res = await RxSubject.RunAsync(token); 
                return res;
            });


            var t2 = Task.Run(async() => { 
                await Task.Delay(500); 
                return new byte[0]; 
            });

            Task<byte[]> completedTask = await Task.WhenAny(t1, t2);

            if (completedTask == t1)
            {
                res = await completedTask;
                
            }

            return res;
        }
    }

}
