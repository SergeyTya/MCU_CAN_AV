using MCU_CAN_AV.Can;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.Devices
{
    internal class BaseDevice : IDevice
    {
        internal BaseDevice() { }

        public ObservableCollection<IDeviceParameter> DeviceDescription = new();
        public ObservableCollection<IDeviceFault> DeviceFaults = new();
        public BehaviorSubject<bool> Init_stage = new(true);

        ObservableCollection<IDeviceParameter> IDevice.DeviceDescriprion => DeviceDescription;

        ObservableCollection<IDeviceFault> IDevice.DeviceFaults => DeviceFaults;

        public Subject<string> LogUpdater => IDevice._LogUpdater;

        BehaviorSubject<bool> IDevice.Init_stage => Init_stage;

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
    }
}
