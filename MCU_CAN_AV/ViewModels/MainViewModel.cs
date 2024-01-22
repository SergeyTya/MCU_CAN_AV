using Avalonia.Threading;
using MCU_CAN_AV.Can;
using MCU_CAN_AV.Models;
using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MCU_CAN_AV.ViewModels;

public class MainViewModel : ViewModelBase
{
    public delegate void MyDelegate();
    public string Greeting => "Welcome to Avalonia!";

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

    public MainViewModel()
    {
        Faults = new ObservableCollection<string>(new List<string>());
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
            });
        });
    }

    void Update(ICAN.RxTxCanData data) {
       

    }
}
