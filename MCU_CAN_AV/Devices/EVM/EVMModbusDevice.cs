using Avalonia.Threading;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Can.ModbusTCP;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using static AsyncSocketTest.ServerModbusTCP;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace MCU_CAN_AV.Devices.EVM_DIAG
{
    internal class EVMModbusDevice : IDevice
    {

        public EVMModbusDevice(ICAN.CANInitStruct InitStruct)
        {
            Init(InitStruct);
        }

        void Init(ICAN.CANInitStruct InitStruct)
        {
       
            Task.Run(async () => {

                var res = await ModbusTCP.ReadRegistersInfoAsync(
                                   server_name: InitStruct.server_name,
                                   server_port: InitStruct.server_port,
                                   modbus_id: InitStruct._devind
                                   ).ConfigureAwait(false);
                return res;
            }).ToObservable().Take(1).Subscribe(

                (_) =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        foreach (var el in _)
                        {
                            IDevice.DeviceDescription.Add(EncodeDeviceDescription(el));
                        }
                    });
                }, () => IDevice.logUpdater.OnNext("Device description reading done") 
                );
        }


        void IDevice.Encode(ICAN.RxTxCanData data)
        {
            //
        }

        void IDevice.Reset()
        {
            throw new NotImplementedException();
        }

        void IDevice.Start()
        {
            throw new NotImplementedException();
        }

        void IDevice.Stop()
        {
            throw new NotImplementedException();
        }

        static EVMModbusTCPDeviceParametr EncodeDeviceDescription(byte[] RXbuf)
        {

            if (RXbuf[7] != 27)
            {
                Debug.WriteLine(String.Format("Holding info response error FC = {0}", RXbuf[7]));
                return null;
            }
            UInt32 hl_adr = BitConverter.ToUInt16(RXbuf.ToArray(), 9);
            UInt32 adr = BitConverter.ToUInt32(RXbuf.ToArray(), 10);
            //Debug.WriteLine(String.Format("HR location = {0}", adr));
            var type = RXbuf[14];
            Debug.WriteLine(String.Format("HR type = {0}", type));
            var index = RXbuf[15];
            //Debug.WriteLine(String.Format("HR index = {0}", index));
            var isRO = RXbuf[16];

            string info = Encoding.UTF8.GetString(RXbuf.ToList().GetRange(17, RXbuf[5] - 11).ToArray());
            Debug.WriteLine(String.Format("HR Info = {0}", info) + " \n");

            var retval = new EVMModbusTCPDeviceParametr(
                        _ID: adr.ToString(),
                        _Name: info,
                        _IsReadWrite: isRO == 0,
                        _Type: null
                    );

            return retval;
        }
    }


    public class EVMModbusTCPDeviceParametr : IDeviceParameter
    {
        string _ID;
        string _Name;
        string _Unit;
        double _Min;
        double _Max;
        bool _IsReadWrite;
        Type _Type;

        

        public EVMModbusTCPDeviceParametr(string _ID, string _Name, bool _IsReadWrite, string _Unit = "", double _Min = 0, double _Max = 0, Type _Type = null)
        {
            this._ID = _ID;
            this._Name = _Name;
            this._Unit = _Unit;
            this._Min = _Min;
            this._Max = _Max;
            this._IsReadWrite = _IsReadWrite;
            this._Type = _Type;
        }

        internal BehaviorSubject<double> Val = new BehaviorSubject<double>(0);
        public IObservable<double> Value { get => Val; }

        string IDeviceParameter.ID => _ID;

        string IDeviceParameter.Name => _Name;

        string IDeviceParameter.Unit => _Unit;

        double IDeviceParameter.Min => _Min;

        double IDeviceParameter.Max => _Max;

        string IDeviceParameter.Type => throw new NotImplementedException();

        List<string> IDeviceParameter.Options => null;

        bool IDeviceParameter.IsReadWrite => _IsReadWrite;

        void IDeviceParameter.writeValue(double value)
        {
            throw new NotImplementedException();
        }
    }
}
