using Avalonia.Xaml.Interactivity;
using MCU_CAN_AV.Can;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using static MCU_CAN_AV.ViewModels.ConnectionState;
using Splat;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Net.Sockets;
using static MCU_CAN_AV.Can.ICAN;

namespace MCU_CAN_AV.Devices
{
    internal class BaseDevice : IDevice, IEnableLogger
    {
        // ************************ IDevice interface ********************************

        public ObservableCollection<IDeviceParameter> DeviceDescription => _DeviceDescription;
        public IObservable<IDeviceFault> DeviceFaults => _DeviceFaults;
        IObservable<string> IDevice.State => _State;
        IObservable<bool> IDevice.Init_stage => _init_stage;
        IObservable<int> IDevice.Connection_errors_cnt => _connection_errors_cnt;
        IObservable<ICAN.RxTxCanData> IDevice.RxData => _RxData;
        IObservable<ICAN.RxTxCanData> IDevice.TxData => _TxData;
        public virtual string Name => throw new NotImplementedException();

        // ***************************************************************************

        internal readonly ICAN.CANInitStruct CANInitStruct;
        internal int _Connection_errors_cnt { set { _connection_errors_cnt.OnNext(value); } }

        internal string _state { set { _State.OnNext(value); } }
        internal bool _Init_stage { set { _init_stage.OnNext(value); } }
        internal IDeviceFault _DeviceFault { set { _DeviceFaults.OnNext(value); } }

        // ***************************************************************************

        private ObservableCollection<IDeviceParameter> _DeviceDescription = new();
        private BehaviorSubject<string> _State = new("no state");
        private BehaviorSubject<bool> _init_stage = new(true);
        private BehaviorSubject<int> _connection_errors_cnt = new(0);
        private Subject<IDeviceFault> _DeviceFaults = new();
        private Subject<ICAN.RxTxCanData> _RxData = new();
        private Subject<ICAN.RxTxCanData> _TxData = new();
        private IDisposable? Rxlistener;
        private readonly ICAN _CAN;

        public BaseDevice(ICAN CAN)
        {
            CANInitStruct = CAN.InitStructure;
            _CAN = CAN;
            Rxlistener = CAN.RxObservable.Subscribe((_) =>
            {
                _RxData.OnNext(_);
                Encode(_);
            });
        }

        internal void TransmitToHardware(ICAN.RxTxCanData _)
        {
            _TxData.OnNext(_);
            _CAN.Transmit(_);
        }

        public void Close()
        {
            Close_instance();
            Rxlistener?.Dispose();
            _CAN.Close();
        }

        protected virtual void Encode(ICAN.RxTxCanData data)
        {
            throw new NotImplementedException();
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }

        public virtual void Start()
        {
            throw new NotImplementedException();
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public virtual void Close_instance()
        {
            throw new NotImplementedException();
        }
    }
}
