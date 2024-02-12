using Avalonia.Controls;
using Avalonia.Threading;
using MCU_CAN_AV.ViewModels;
using System;
using System.Linq;

namespace MCU_CAN_AV.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        this.Width = 800;

    }
}
