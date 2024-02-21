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

namespace MCU_CAN_AV.Devices
{
    internal class BaseDevice : IDevice
    {

        public ObservableCollection<IDeviceParameter> DeviceDescription => IDevice._DeviceDescription;

        // public ObservableCollection<IDeviceFault>  DeviceFaults => IDevice._DeviceFaults;

        public Subject<IDeviceFault> DeviceFaults => IDevice._DeviceFaults;
        public IDeviceFault _DeviceFault { set { DeviceFaults.OnNext(value); } }

        public BehaviorSubject<string> State => IDevice._State;
        internal string _state { set { State.OnNext(value); } }

        BehaviorSubject<bool> IDevice.Init_stage => IDevice._Init_stage;
        internal bool _Init_stage { set { IDevice._Init_stage.OnNext(value); } }

        public BehaviorSubject<int> Connection_errors_cnt => IDevice._Connection_errors_cnt;

        public int _Connection_errors_cnt { set { Connection_errors_cnt.OnNext(value); } }

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

        public virtual void Close() {  
            throw new NotImplementedException(); 
        }

    }
}
