using Avalonia;
using Avalonia.Controls;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using ReactiveUI;

namespace MCU_CAN_AV.CustomControls
{

    internal class CustomSliderViewModel : ReactiveObject
    {

        private string caption;
        public string Caption
        {
            get => caption;
            set => this.RaiseAndSetIfChanged(ref caption, value);
        }

    }
}
