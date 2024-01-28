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

        public static readonly StyledProperty<ObservableCollection<string>> FaultTableProperty =
          AvaloniaProperty.Register<FaultsIndi, ObservableCollection<string>>("FaultTable");
        public ObservableCollection<string> FaultTable
        {
            set => SetValue(FaultTableProperty, value);
            get => GetValue(FaultTableProperty);
        }

        public FaultsIndi()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property.Name == "FaultTable")
            {
                // Bind data to dataGrid from property
                if (DataGrid_Faults.ItemsSource != FaultTable)
                {
                    DataGrid_Faults.ItemsSource = FaultTable;
                }
            }
            base.OnPropertyChanged(change);
        }
    }
}
