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
using Avalonia.Media;

namespace MCU_CAN_AV.Views
{
    public partial class MainView3 : UserControl
    {
        public MainView3()
        {
            InitializeComponent();
            Log.PropertyChanged += Log_PropertyChanged;
            Checkbox_saver.Click += Checkbox_saver_Click;
        }

        private void Checkbox_saver_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Checkbox_saver.IsChecked == true)
            {
                Border_Main.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                Border_Main.BorderBrush = new SolidColorBrush(Colors.Black);
            }
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
