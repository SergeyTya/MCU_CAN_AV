using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Emit;
using System.Security.Cryptography;

namespace MCU_CAN_AV.CustomControls
{
    public partial class CustomSlider : Panel
    {
        BehaviorSubject<int> ValueSubject = new BehaviorSubject<int>(0);

        public static readonly StyledProperty<IObservable<int>> ValueProperty =
            AvaloniaProperty.Register<CustomSlider, IObservable<int>>("Value");
        public IObservable<int> Value
        {
            set => SetValue(ValueProperty, value); 
            get => GetValue(ValueProperty);
        }

        public static readonly StyledProperty<double> MinProperty =
           AvaloniaProperty.Register<CustomSlider, double>("Min");
        public double Min
        {
            set
            {
                slider.Minimum = value;
                SetValue(MinProperty, value);
            }
            get => GetValue(MinProperty);
        }

        public static readonly StyledProperty<double> MaxProperty =
            AvaloniaProperty.Register<CustomSlider, double>("Max");
        public double Max
        {
            set
            {
                slider.Maximum = value;
                SetValue(MaxProperty, value);
            }
            get => GetValue(MaxProperty);
        }

        public static readonly StyledProperty<string> LabelTextProperty =
           AvaloniaProperty.Register<CustomSlider, string>("LabelText");
        public string LabelText
        {
            set =>SetValue(LabelTextProperty, value);
            get => GetValue(LabelTextProperty);
        }


        public CustomSlider()
        {
            
            InitializeComponent();

            textbox.TextChanged += Textbox_TextChanged;
            textbox.KeyUp += Textbox_KeyUp;

            this.Value = ValueSubject; 
            slider.ValueChanged += (_,__) => {
                /* Value.Add(slider.Value);*/
                ValueSubject.OnNext((int) slider.Value); 
               
            };
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
