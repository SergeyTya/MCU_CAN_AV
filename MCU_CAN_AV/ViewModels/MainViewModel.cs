using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.CustomControls;
using MCU_CAN_AV.DeviceDescriprion;
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
    public IObservable<int> slider_speed_observer;

    public int slider_speed;
    public int slider_torque;


    public ObservableCollection<string> Faults { get; } = new();

   
    double temp = 0;
    public MainViewModel()
    {
      
        if (slider_speed_observer != null) {
            slider_speed_observer.Subscribe(
        //x => slider_speed = x
            (_) => {
            Debug.WriteLine("get slider" + _);
        }
        );

        }

    }

}
