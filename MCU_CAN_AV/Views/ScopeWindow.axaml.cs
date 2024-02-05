using Avalonia;
using Avalonia.Controls;
using MCU_CAN_AV.CustomControls;

namespace MCU_CAN_AV.Views
{
    public partial class ScopeWindow : Window
    {

        public static readonly StyledProperty<string> InputValueProperty =
          AvaloniaProperty.Register<MetersControl, string>("InputValue");
        public string InputValue
        {
            set => SetValue(InputValueProperty, value);
            get => GetValue(InputValueProperty);
        }
        public ScopeWindow(string name)
        {
            Name = name;    
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            var new_value = change.NewValue;

            if (change.Property.Name == "InputValue")
            {
                TB.Text = InputValue;
            }
        }
    }
}
