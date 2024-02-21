using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using ScottPlot.Avalonia;
using ScottPlot.Drawing.Colormaps;
using System;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace utils
{

 
    internal class ScottPlotGraph
    {

        private AvaPlot ScopeView;
        private readonly DispatcherTimer _renderTimer;
        public ScopeChannels ScopeChannels;
        static double Ts = 0.001;
        string Name;

        public ScottPlotGraph(
            AvaPlot ScopeView,
            int channel_cnt = 1,
            double span = 1.0,
            double ts = 0.001,
            string? name = null
            ) {
            Ts = ts;
            this.ScopeView = ScopeView;
            Name = name;

            int data_lenght = (int) (span/ ts);
            ScopeChannels = new ScopeChannels(1);

            RebuildScope(channel_cnt, data_lenght);

            _renderTimer = new DispatcherTimer();
            _renderTimer.Interval = TimeSpan.FromMilliseconds(300);
            _renderTimer.Tick += Render;
            _renderTimer.Start();
           
        }

        private void Render(object sender, EventArgs e)
        {
            ScopeView.Refresh();
            ScopeView.Plot.AxisAutoY(margin: 0.5);
        }

        private void RebuildScope(int channel_cnt, int data_lenght) {
            ScopeChannels.Capacity = channel_cnt;
            ScopeView.Plot.Clear();

            for (int i = 0; i < channel_cnt; i++)
            {
                ScopeChannels[i].Capacity = data_lenght;
                var tmp = ScopeChannels[i].Points;
                var plt1 = ScopeView.Plot.AddSignal(tmp, color: System.Drawing.Color.Yellow);
                ScopeView.Plot.AxisAutoX(margin: 0);
                ScopeView.Plot.SetAxisLimits(yMin: -2, yMax: 2);
                ScopeView.Plot.Style(ScottPlot.Style.Black);
                plt1.YAxisIndex = 0;
                plt1.LineWidth = 1;
            }
            ScopeView.Plot.XAxis.TickLabelFormat(customTickFormatter);
            ScopeView.Plot.Title(Name);
        }

        static string customTickFormatter(double position)
        {
            position *= Ts;
            return position.ToString("#0.0##");
        }

      
    }
}
