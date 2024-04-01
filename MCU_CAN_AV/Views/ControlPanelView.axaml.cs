using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using MCU_CAN_AV.Devices;
using System.Collections.ObjectModel;

namespace MCU_CAN_AV.Views
{
    public partial class ControlPanelView : UserControl
    {

        public static readonly StyledProperty<bool> IsControlEnabledProperty =
         AvaloniaProperty.Register<ControlPanelView, bool>("IsControlEnabled");

        public bool IsControlEnabled
        {
            set => SetValue(IsControlEnabledProperty, value);
            get => GetValue(IsControlEnabledProperty);
        }

        public ControlPanelView()
        {
            InitializeComponent();
            TB_UnlockControl.Click += (_, __) => {
                if (TB_UnlockControl.IsChecked != null) {
                    IsControlEnabled = !(bool) TB_UnlockControl.IsChecked;
                }
            };
        }
    }
}
