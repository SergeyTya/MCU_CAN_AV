using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SkiaSharp;
using System.Collections.Specialized;
using System.Collections.Generic;
using ReactiveUI;
using Avalonia;
using System.Collections.ObjectModel;

namespace MCU_CAN_AV.CustomControls
{
    public partial class FaultsIndi : UserControl
    {

        public static readonly StyledProperty<ObservableCollection<MCU_CAN_AV.Devices.IDeviceFault>> FaultTableProperty =
          AvaloniaProperty.Register<FaultsIndi, ObservableCollection<MCU_CAN_AV.Devices.IDeviceFault>>("FaultTable");
        public ObservableCollection<MCU_CAN_AV.Devices.IDeviceFault> FaultTable
        {
            set {
                SetValue(FaultTableProperty, value);
                DataGrid_Faults.ItemsSource = FaultTable;
            }
            get => GetValue(FaultTableProperty);
        }

        public FaultsIndi()
        {
            InitializeComponent();
        }

    }
}
