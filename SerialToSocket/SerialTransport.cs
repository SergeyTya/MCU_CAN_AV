using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialToSocket
{
    internal interface ISerialTransport
    {
        Task<byte[]?> SerialWrite(int ID, byte[] data, CancellationToken token);
    }

    public class SerialTransport : ISerialTransport, IEnableLogger
    {
        SerialPort _serial;
        Subject<byte[]> RxSubject = new();
        public SerialTransport(SerialPort serial)
        {
            _serial = serial;
        }

        SemaphoreSlim _semaphore = new(1);

        public async Task<byte[]?> SerialWrite(int ID, byte[] data, CancellationToken token)
        {
            try
            {

                if (_serial == null) return null;
                if (!_serial.IsOpen) return null;


                await _semaphore.WaitAsync(token);
                try
                {
                    _serial.Write(data, 0, data.Length);

                    this.Log().Info($"ID:{ID} Write to serial: " + BitConverter.ToString(data));

                    int BytesToRead = 0;
                    int timeout = 0;
                    while (timeout++ < 3)
                    {
                        int BytesToReadNow = _serial.BytesToRead;
                        if (BytesToRead > 0)
                        {
                            if (BytesToReadNow == BytesToRead)
                            {
                                byte[] buff = new byte[BytesToReadNow];
                                _serial.Read(buff, 0, _serial.BytesToRead);
                                this.Log().Info($"ID:{ID} Read from serial: " + BitConverter.ToString(buff));
                                return buff;
                            }
                        }
                        BytesToRead = BytesToReadNow;
                        await Task.Delay(5, token); ;
                    }
                    _serial.Read(new byte[_serial.BytesToRead], 0, _serial.BytesToRead);
                    this.Log().Error($"ID:{ID} Serial Timeout");
                    return null;

                }
                finally
                {
                    _semaphore.Release();
                }

            }
            catch (TaskCanceledException ex)
            {
                this.Log().Error($"ID:{ID} TaskCanceledException ");
                return null;
            }
            catch (OperationCanceledException ex)
            {
                this.Log().Error($"ID:{ID} OperationCanceledException ");
                return null;
            }

        }
    }
}
