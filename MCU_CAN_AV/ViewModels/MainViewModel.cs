using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.CustomControls;
using MCU_CAN_AV.Models;
using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata;

namespace MCU_CAN_AV.ViewModels;

public class MainViewModel : ViewModelBase
{
    public IObservable<int> slider_speed_observer;

    public int slider_speed;
    public int slider_torque;


    public string _id;
    public string? Id
    {
        get
        {
            return _id;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _id, value);
        }
    }


    public ObservableCollection<string> Faults { get; }
    public ObservableCollection<MCU_CAN_AV.CustomControls.ControlTable.Parameter> TableOfControls { get; }

    public MainViewModel()
    {

        Faults = new ObservableCollection<string>(new List<string>());
        TableOfControls = new ObservableCollection<MCU_CAN_AV.CustomControls.ControlTable.Parameter>(new List<MCU_CAN_AV.CustomControls.ControlTable.Parameter>());


        var tester = new tester();
        MCU_CAN_AV.CustomControls.ControlTable.Parameter.Type[] types = {
            MCU_CAN_AV.CustomControls.ControlTable.Parameter.Type.BUTTON,
            MCU_CAN_AV.CustomControls.ControlTable.Parameter.Type.LIST,
            MCU_CAN_AV.CustomControls.ControlTable.Parameter.Type.TEXT };

        IDisposable listener = tester.updater.Subscribe(
        (_) =>
        {
            Debug.WriteLine(_.id);
            Id = _.id.ToString();

            //Update MetterFaultTable
            Dispatcher.UIThread.Invoke(() => {
                this.Faults.Add("fault"+_.id);
                if (this.Faults.Count > 10) this.Faults.Clear();

                if (this.TableOfControls.Count < 3) {

                    TableOfControls.Add(new MCU_CAN_AV.CustomControls.ControlTable.Parameter(
                        TableOfControls.Count,
                        "hhh" + TableOfControls.Count,
                        types[TableOfControls.Count]
                        ));
                }
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
}
