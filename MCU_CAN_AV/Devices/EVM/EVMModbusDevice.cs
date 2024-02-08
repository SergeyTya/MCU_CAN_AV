﻿using AsyncSocketTest;
using Avalonia.Threading;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Can.ModbusTCP;
using MCU_CAN_AV.Properties;
using MCU_CAN_AV.utils;
using Newtonsoft.Json;
using ReactiveUI;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AsyncSocketTest.ServerModbusTCP;
using static MCU_CAN_AV.Devices.Shanghai.ShanghaiDevice;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace MCU_CAN_AV.Devices.EVM_DIAG
{
    internal class EVMModbusDevice : IDevice
    {
        static List<EVMModbusTCPDeviceFault> FaultsList = new();
        public EVMModbusDevice(ICAN.CANInitStruct InitStruct)
        {
            Init(InitStruct);
        }

        void Init(ICAN.CANInitStruct InitStruct)
        {

            Task.Run(async () =>
            {
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
                        EncodeDeviceDescription(_);
                    });
                },
                exeption => { ICAN.LogUpdater.OnNext(exeption.Message); },
                () => IDevice.logUpdater.OnNext("Device description reading done")
            );


            try
            {
                string fileContents = IDevice.ReadJsonFromResources(Resources.EVM_faults);
                FaultsList = JsonConvert.DeserializeObject<List<EVMModbusTCPDeviceFault>>(fileContents);

            }
            catch (JsonReaderException e)
            {
                ICAN.LogUpdater.OnNext(e.Message);
            }
        }

        void IDevice.Encode(ICAN.RxTxCanData data)
        {
            if (data.id == 1 || data.id == 2)
            {

                BitArray bits = new BitArray(data.data);

                for (int i = 0; i < 16; i++)
                {
                    if (bits[i] == true)
                    {
                        var res = FaultsList
                            .Where(x => x.adr == (i + (data.id - 1) * 16))
                            .Where(x => IDevice.DeviceFaults.IndexOf((IDeviceFault)x) == -1);

                        foreach (var x in res) { IDevice.DeviceFaults.Add(res); }
                    }
                }
                return;
            }

            foreach (EVMModbusTCPDeviceParametr item in IDevice.DeviceDescription)
            {
                foreach (var id in item._ids)
                {
                    if (data.id == id)
                    {

                        UInt16[] tmp = { 0 };

                        Buffer.BlockCopy(data.data, 0, tmp, 0, 2);

                        item.data[item._ids.IndexOf(id)] = tmp[0];

                        if (item._ids.Last() == id)
                        {

                            UInt32[] tmp2 = { 0 };
                            Buffer.BlockCopy(item.data.ToArray(), 0, tmp2, 0, 4);

                            UInt32 val0 = tmp2[0];
                            double ret_val = 0;
                            switch (item._Type)
                            {
                                case HoldingType.HTYPE_INT16:
                                    ret_val = (Int16)val0;
                                    break;
                                case HoldingType.HTYPE_UINT16:
                                    ret_val = (UInt16)val0;
                                    break;
                                case HoldingType.HTYPE_INT32:
                                    ret_val = (Int32)val0;
                                    break;
                                case HoldingType.HTYPE_UINT32:
                                    ret_val = (UInt32)val0;
                                    break;
                                case HoldingType.HTYPE_FLOAT:
                                    ret_val = (float)BitConverter.ToSingle(BitConverter.GetBytes(val0), 0);
                                    break;
                                default:
                                case HoldingType.NONE:
                                    break;
                            }

                            if (item._val != ret_val)
                            {
                                item.Val.OnNext(ret_val);
                                item._val = ret_val;
                            }
                        }
                    }
                }
            }
        }

        void IDevice.Reset()
        {
            IDevice.DeviceFaults.Clear();
            ICAN.TxUpdater.OnNext(new ICAN.RxTxCanData(0, new byte[] { 4, 0 }));
            IDevice.logUpdater.OnNext("Reset command");

        }

        void IDevice.Start()
        {
            ICAN.TxUpdater.OnNext(new ICAN.RxTxCanData(0, new byte[] { 1, 0 }));
            IDevice.logUpdater.OnNext("Start command");
        }

        void IDevice.Stop()
        {
            ICAN.TxUpdater.OnNext(new ICAN.RxTxCanData(0, new byte[] { 2, 0 }));
            IDevice.logUpdater.OnNext("Stop command");
        }


        static void EncodeDeviceDescription(List<byte[]> data)
        {
            // skip firs 3 items
            for (uint i = 3; i < data.Count; i++)
            {

                var RXbuf = data[(int)i];

                if (RXbuf[7] != 27)
                {
                    Debug.WriteLine(String.Format("Holding info response error FC = {0}", RXbuf[7]));
                    return;
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

                var tmp = new EVMModbusTCPDeviceParametr(
                            _ID: i.ToString(),
                            _Name: info,
                            _IsReadWrite: isRO == 0,
                            _Type: (HoldingType)type
                        );

                if (
                     (HoldingType)type != HoldingType.HTYPE_INT16
                  && (HoldingType)type != HoldingType.HTYPE_UINT16
                 )
                {
                    tmp._ids = new uint[] { i, ++i };
                }
                else
                {
                    tmp._ids = new uint[] { i };
                }

                IDevice.DeviceDescription.Add(tmp);
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


            internal HoldingType _Type;
            internal UInt16[] data = new UInt16[4];
            internal uint[] _ids;
            internal double _val;

            public EVMModbusTCPDeviceParametr(string _ID, string _Name, bool _IsReadWrite, HoldingType _Type, string _Unit = "", double _Min = 0, double _Max = 0)
            {
                this._ID = _ID;
                this._Name = _Name;
                this._Unit = _Unit;
                this._Min = _Min;
                this._Max = _Max;
                this._IsReadWrite = _IsReadWrite;
                this._Type = _Type;
            }

            public BehaviorSubject<double> Val = new BehaviorSubject<double>(0);

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
                byte[] bval = GetByteFromString(value.ToString(), _Type);
                if (bval == null) return;
                IDevice.logUpdater.OnNext($"reg#{_ID} -> {value}");
                int count = 0;
                int id = 0;
                var buf = bval.ToList()
                    .GroupBy(_ => count++ / 2)
                    .Select(v => new ICAN.RxTxCanData((_ids[id++]), v.ToArray()));

                foreach (var item in buf)
                {
                    ICAN.TxUpdater.OnNext(item);
                };
                _val = 0;
            }
        }

        public static byte[] GetByteFromString(string str, HoldingType type)
        {

            byte[] ret_val = null;
            switch (type)
            {
                case HoldingType.HTYPE_INT16:
                    if (Int16.TryParse(str, out Int16 res_val) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val);
                    break;
                case HoldingType.HTYPE_UINT16:
                    if (UInt16.TryParse(str, out UInt16 res_val1) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val1);
                    break;
                case HoldingType.HTYPE_INT32:
                    if (Int32.TryParse(str, out Int32 res_val2) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val2);
                    break;
                case HoldingType.HTYPE_UINT32:
                    if (UInt32.TryParse(str, out UInt32 res_val3) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val3);
                    break;
                case HoldingType.HTYPE_FLOAT:
                    if (Single.TryParse(str, out float res_val4) == false) return ret_val;
                    ret_val = BitConverter.GetBytes(res_val4);
                    break;
                default:
                case HoldingType.NONE:
                    break;
            }
            return ret_val;
        }

        internal class EVMModbusTCPDeviceFault : IDeviceFault
        {
            public string ID => adr.ToString();

            public string Name => name;

            [JsonProperty("adr")]
            internal uint adr;

            [JsonProperty("name")]
            internal string name;
        }
    }

    public enum HoldingType : byte
    {
        HTYPE_UINT16,
        HTYPE_INT16,
        HTYPE_UINT32,
        HTYPE_INT32,
        HTYPE_FLOAT,
        NONE = 0xff
    }

}