using Avalonia;
using Avalonia.Controls;
using MCU_CAN_AV.Can;
using System;
using System.Diagnostics;

namespace MCU_CAN_AV.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        this.DataContext = new MCU_CAN_AV.ViewModels.MainViewModel();
    }
}
