using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using MCU_CAN_AV.CustomControls;
using MCU_CAN_AV.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MCU_CAN_AV.Views
{
    public partial class ConnectionView : UserControl
    {
     
        public static readonly StyledProperty<int> SelectedTabProperty =
        AvaloniaProperty.Register<ConnectionView, int>("SelectedTab");
        public int SelectedTab
        {
            set
            {
                SetValue(SelectedTabProperty, value);
            }
            get => GetValue(SelectedTabProperty);
        }

        public ConnectionView()
        {
            InitializeComponent();

        }
    }
}
