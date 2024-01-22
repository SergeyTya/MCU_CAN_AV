using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCU_CAN_AV.ViewModels
{

    internal class MetersViewModel : ViewModelBase
    {

        public class Fault
        {
            public string Name { get; set; }
            public Fault(string name)
            {
                this.Name = name;
            }
        }
        public ObservableCollection<Fault> Faults { get; }
        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements_trq { get; set; }
        public NeedleVisual Needle_trq { get; set; }

        public IEnumerable<VisualElement<SkiaSharpDrawingContext>> VisualElements_spd { get; set; }
        public NeedleVisual Needle_spd { get; set; }


        public MetersViewModel()
        {
            init_angular();
            Faults = new ObservableCollection<Fault>(new List<Fault>());

            //Label_Torque.Text = string.Format("Torque {0} %", 10.2);
            //Label_Speed.Text = string.Format("Speed {0} krpm", 1.2);

            //Label_Current.Text = string.Format("Current {0} A", 10.2);
            //Label_Voltage.Text = string.Format("Voltage {0} %", 1.2);

        }

        void init_angular() {

            Needle_trq = new NeedleVisual { Value = 45 };
            Needle_spd = new NeedleVisual { Value = 3 };

            VisualElements_trq = new VisualElement<SkiaSharpDrawingContext>[]
            {
                new AngularTicksVisual{
                    LabelsSize = 16,
                    LabelsOuterOffset = 15,
                    OuterOffset = 65,
                    TicksLength = 20
                },
                Needle_trq
            };

            VisualElements_spd = new VisualElement<SkiaSharpDrawingContext>[]
{
                new AngularTicksVisual{
                    LabelsSize = 16,
                    LabelsOuterOffset = 15,
                    OuterOffset = 65,
                    TicksLength = 20
                },
                Needle_spd
            };
        }
    }
}
