using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace MCU_CAN_AV.Views
{
    public partial class ParamTableView : UserControl
    {
        public ParamTableView()
        {
            InitializeComponent();
        }


        void SwitchFocusEvent(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("sd");
        }
        /*
         
        <TextBox.KeyBindings>
											<KeyBinding Gesture="Enter" Command="{Binding Write}" />
										</TextBox.KeyBindings>	

         */
    }
}
