using Avalonia.Controls;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.CustomControls;
using Avalonia.Data;
using ReactiveUI;
using System.Reactive.Linq;
using Avalonia;
using System.Collections.Specialized;
using System.Diagnostics;
using MCU_CAN_AV.ViewModels;

namespace MCU_CAN_AV.Views
{
    public partial class MainView3 : UserControl
    {
        public MainView3()
        {
            InitializeComponent();
             Log.PropertyChanged += Log_PropertyChanged;
          
        }

        private void Log_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if(e.Property == ItemsControl.ItemCountProperty)
            {
                ScrollLog.ScrollToEnd();
            }
        }
    }
}
