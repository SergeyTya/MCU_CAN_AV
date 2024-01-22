using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace MCU_CAN_AV.CustomControls
{
    public partial class CustomSlider : Panel
    {


        public static readonly StyledProperty<string> LabelTextProperty  =
            AvaloniaProperty.Register<CustomSlider, string>("LabelText");


        public double Min
        {
            set
            {
                slider.Minimum = value;
            }
        }
        public double Max
        {
            set
            {
                slider.Maximum = value;
            }
        }

        public string LabelText
        {
            set
            {
                SetValue(LabelTextProperty, value);
            }

            get
            {
                return GetValue(LabelTextProperty);
            }
        }


        public CustomSlider()
        {
            InitializeComponent();
            textbox.TextChanged += Textbox_TextChanged;
            textbox.KeyUp += Textbox_KeyUp;
        }

        private void Textbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Escape)
            {
                for (int i = 0; i < ((TextBox)sender).UndoLimit; i++)
                {
                    ((TextBox)sender).Undo();
                }

            }
            if (e.Key != Avalonia.Input.Key.Enter) return;


            var str = ((TextBox)sender).Text;
            var val = check_value(str);
            ((TextBox)sender).Text = val.ToString("0");
            slider.Value = val;
        }

        private void Textbox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var str = ((TextBox)sender).Text;
            if (str == "") return;
            ((TextBox)sender).Text = check_value(str).ToString("0");

        }

        private double check_value(string str)
        {

            double val = 0;
            bool res = double.TryParse(str, out val);
            if (!res)
            {
                val = slider.Value;
            }
            return val;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {


            if (change.Property.Name == "LabelText")
            {
                label.Text = LabelText;
            }

            base.OnPropertyChanged(change);
        }
    }
}