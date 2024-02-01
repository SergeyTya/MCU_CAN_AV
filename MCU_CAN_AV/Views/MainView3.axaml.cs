using Avalonia.Controls;
using MCU_CAN_AV.DeviceDescriprion;
using MCU_CAN_AV.CustomControls;
using Avalonia.Data;
using ReactiveUI;
using System.Reactive.Linq;
using Avalonia;

namespace MCU_CAN_AV.Views
{
    public partial class MainView3 : UserControl
    {
        public MainView3()
        {
            InitializeComponent();

            var binding = new Binding {

                Source = DeviceDescriptionReader.DeviceDescription,
                Path = nameof(DeviceDescriptionReader.DeviceDescription)

            };

            controlTable.Bind(ControlTable.TableSourceProperty, binding);

        }
    }
}
