using CommunityToolkit.Mvvm.ComponentModel;
using MCU_CAN_AV.Devices;
using ReactiveUI;
using System.Diagnostics;

namespace MCU_CAN_AV.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public bool _isConnectionDone = true;
 
    public ViewModelBase() {
        Debug.WriteLine("ViewModelBase ");
    }


}
