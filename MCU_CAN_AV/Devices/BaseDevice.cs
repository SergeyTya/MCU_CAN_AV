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

namespace MCU_CAN_AV.Devices
{
    internal class BaseDevice : IDevice, IEnableLogger
    {
        internal readonly ICAN _CAN;

        internal ObservableCollection<IDeviceParameter> _DeviceDescription = new();
        
        internal BehaviorSubject<string> _State = new("no state");
        
        internal BehaviorSubject<bool> _init_stage = new(true);
        
        internal BehaviorSubject<int> _connection_errors_cnt = new(0);
        
        internal Subject<IDeviceFault> _DeviceFaults = new();

        static IDisposable? Rxlistener;

        public BaseDevice(ICAN CAN)
        {

            _CAN = CAN;
            Rxlistener = CAN.RxObservable.Subscribe((_) => Encode(_));
        }

        public ObservableCollection<IDeviceParameter> DeviceDescription => _DeviceDescription;

        // public ObservableCollection<IDeviceFault>  DeviceFaults => IDevice._DeviceFaults;

        public IObservable<IDeviceFault> DeviceFaults => _DeviceFaults;
        public IDeviceFault _DeviceFault { set { _DeviceFaults.OnNext(value); } }

        public IObservable<string> State => _State;
        internal string _state { set { _State.OnNext(value); } }

        IObservable<bool> IDevice.Init_stage => _init_stage;
        internal bool _Init_stage { set { _init_stage.OnNext(value); } }

        public IObservable<int> Connection_errors_cnt => _connection_errors_cnt;

        public int _Connection_errors_cnt { set { _connection_errors_cnt.OnNext(value); } }



        public virtual string Name => throw new NotImplementedException();


        public virtual void Encode(ICAN.RxTxCanData data)
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


        public void Close()
        {
            Close_instance();
            Rxlistener?.Dispose();
            _CAN.Close();
        }

    }
}
