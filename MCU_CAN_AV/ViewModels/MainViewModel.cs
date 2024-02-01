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


    public string _id;
    public string? Id { get{return _id;} set { this.RaiseAndSetIfChanged(ref _id, value);} }

    bool table_init = false;

    public ObservableCollection<string> Faults { get; } = new();

    public ObservableCollection<DeviceParameter> TableOfControls { get; } = new();

   
    double temp = 0;
    public MainViewModel()
    {


        TableOfControls = DeviceDescriptionReader.DeviceDescription;

        var tester = new tester();


        IDisposable listener = tester.updater.Subscribe(
        (_) =>
        {
            Debug.WriteLine(_.id);
            Id = _.id.ToString();

            //Update MetterFaultTable
            Dispatcher.UIThread.Invoke(() => {
                this.Faults.Add("fault"+_.id);
                if (this.Faults.Count > 10) this.Faults.Clear();

                //if(TableOfControls.Count == 0)
                //{
                //    foreach (DeviceParameter param in DeviceDescriptionReader.DeviceDescription) {
                //        TableOfControls.Add(param); 
                //    }

                //}

            });
        });

        

        if (slider_speed_observer != null) {
            slider_speed_observer.Subscribe(
        //x => slider_speed = x
            (_) => {
            Debug.WriteLine("get slider" + _);
        }
        );

        }

    }

    void Update(ICAN.RxTxCanData data) {
       

    }

    public void set_visible1() {

      
    }

    public void set_visible2()
    {


    }
}
