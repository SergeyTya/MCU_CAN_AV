using MCU_CAN_AV.Can;
using MCU_CAN_AV.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MCU_CAN_AV.ViewModels;

public class MainViewModel : ViewModelBase
{
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

    public MainViewModel()
    {
       
        var tester = new tester();

        IDisposable listener = tester.updater.Subscribe(
        (_) =>
        {
            Debug.WriteLine(_.id);
            Id = _.id.ToString();
        });
    }

    void Update(ICAN.RxTxCanData data) { 
    
    }
}
