using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.CustomControls;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.Models;
using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Metadata;

namespace MCU_CAN_AV.ViewModels;

public class MainViewModel : ViewModelBase
{

    bool _IsConnVisible = true;
    public bool IsConnVisible
    {
        set
        {
            this.RaiseAndSetIfChanged(ref _IsConnVisible, value);
        }
        get { return _IsConnVisible; }
    }

    public void OnClickConnectCommand()
    {
        Debug.WriteLine("aass");
        MCU_CAN_AV.Devices.Shanghai.ShanghaiDevice.Init();
        IsConnVisible = false;
    }

  
}
