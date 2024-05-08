using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MCU_CAN_AV.CustomControls
{
    public partial class CustomSlider : Panel
    {
  
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<CustomSlider, double>("Value");
        public double Value
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


        int cntr = 0;
        IAsyncResult? SliderMoveFinished;

        public CustomSlider()
        {
            
            InitializeComponent();

            textbox.TextChanged += Textbox_TextChanged;
            textbox.KeyUp += Textbox_KeyUp;

            textbox.KeyDown += Textbox_KeyDown; ;

            slider.ValueChanged += (_,__) => {

                if (SliderMoveFinished == null || SliderMoveFinished.IsCompleted)
                {
                    SliderMoveFinished = Task.Run(async () => { 
                        await Task.Delay(100);
                        Dispatcher.UIThread.Post(() =>
                        {
                            this.Value = slider.Value;
                        });
                    });
                }
            };
        }

        private void Textbox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Up)
            {
                Value += Max * 0.01;
                if (Value > Max) Value = Max;
                slider.Value = Value;
            };


            if (e.Key == Avalonia.Input.Key.Down)
            {
                Value -= Max * 0.01;
                if (Value < Min) Value = Min;
                slider.Value = Value;
            };
        }

        private void Textbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (sender == null) return;

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
            if (sender == null) return;
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

            if (change.Property.Name == "Min")
            {
                slider.Minimum = Min;
            }


            if (change.Property.Name == "Max")
            {
                slider.Maximum = Max;
            }

            base.OnPropertyChanged(change);
        }
    }
}
