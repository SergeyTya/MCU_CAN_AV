using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
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

    bool table_init = false;

    public ObservableCollection<string> Faults { get; }
    public ObservableCollection<MCU_CAN_AV.CustomControls.ControlTable.Parameter> TableOfControls { get; }

    public MainViewModel()
    {
        Faults = new ObservableCollection<string>(new List<string>());
        TableOfControls = new ObservableCollection<MCU_CAN_AV.CustomControls.ControlTable.Parameter>(new List<MCU_CAN_AV.CustomControls.ControlTable.Parameter>());

        DeviceDescriprion.DeviceDescriptionReader.Read();

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

                if (!table_init)
                {
                    foreach (var el in DeviceDescriptionReader.ShanghaiDevice)
                    {
                        ControlTable.Parameter.Type type = ControlTable.Parameter.Type.TEXT;
                        if (el.options != null)
                        {
                            type = ControlTable.Parameter.Type.LIST;
                        }

                        var param = new MCU_CAN_AV.CustomControls.ControlTable.Parameter(
                                     el.CANID,
                                     el.sname,
                                     items: el.options,
                                     type: type,
                                     writeEnable: el.RW
                                     );
                        param.Value = 0;
                        param.onValueChangedByUser += (_, __) =>
                        {

                            Debug.WriteLine("Value " + Convert.ToDouble(_));
                        };

                        TableOfControls.Add(param);
                    }

                    table_init = true;
                }
                else {

                    TableOfControls[3].Value = _.id;
                    TableOfControls[10].Value = _.id;
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
