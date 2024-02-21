using Avalonia;
using Avalonia.Controls;
using MCU_CAN_AV.CustomControls;
using ScottPlot;
using ScottPlot.Avalonia;
using System.Reactive.Linq;
using System;
using utils;
using Avalonia.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace MCU_CAN_AV.Views
{

    public partial class Logger2Window : Window
    {
        public static readonly StyledProperty<string> InputValueProperty =
          AvaloniaProperty.Register<MetersControl, string>("InputValue");

        public string InputValue
        {
            set
            {
                SetValue(InputValueProperty, value);
            }
            get => GetValue(InputValueProperty);
        }


        public Logger2Window(string Name)
        {
            this.Name = Name;
            InitializeComponent();
            this.Title = Name;
          
            this.Closed += (_,__) => {
                _Is_Alive = false;
                this.Close();
            };

            double time_step = 0.1;
            
            ScottPlotGraph scope = new ScottPlotGraph(
                avaPlot_Scope, 
                channel_cnt: 1, 
                span:10.0, 
                ts: time_step,
                name: Name
            );

            Observable.Interval(TimeSpan.FromSeconds(time_step)).Subscribe(x => {
                Dispatcher.UIThread.Post((() =>
                {
                    scope.ScopeChannels.Update(new double[] { Double.Parse(InputValue) });
                }));
            });
        }

        bool _Is_Alive = true;
        public bool Is_Alive { get => _Is_Alive; }
    }
}
