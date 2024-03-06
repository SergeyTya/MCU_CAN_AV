using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Avalonia;
using System;

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
            ScopeView.Plot.Axes.AutoScaleY();
            ScopeView.Refresh();
         
        }

        private void RebuildScope(int channel_cnt, int data_lenght) {
            ScopeChannels.Capacity = channel_cnt;
            ScopeView.Plot.Clear();
         //  ScopeView.Plot.Add.Palette = new ScottPlot.Palettes.Dark();

            ScopeView.Plot.Style.ColorAxes(Color.FromHex("#d7d7d7"));
            ScopeView.Plot.Style.ColorGrids(Color.FromHex("#404040"));
            ScopeView.Plot.Style.Background(
            //    figure: Color.FromHex("#181818"),
                figure: Colors.Black,
                data:   Color.FromHex("#1f1f1f"));
            ScopeView.Plot.Style.ColorLegend(
                background: Color.FromHex("#404040"),
                foreground: Color.FromHex("#d7d7d7"),
                border: Color.FromHex("#d7d7d7"));



            for (int i = 0; i < channel_cnt; i++)
            {
                ScopeChannels[i].Capacity = data_lenght;
                var tmp = ScopeChannels[i].Points;
                var plt1 = ScopeView.Plot.Add.Signal(tmp);
                plt1.LineStyle.Color = Colors.Yellow;
                plt1.LineStyle.Pattern = LinePattern.Solid;
                plt1.LineWidth = 2;
                plt1.Data.Period = Ts;

                DateTime start = new(2024, 1, 1);
                

            }
            //ScopeView.Plot.Title(Name);
            ScopeView.Plot.Axes.Title.Label.Text = Name;
           // ScopeView.Plot.Axes.Title.Label.

        }

       
    }
}
