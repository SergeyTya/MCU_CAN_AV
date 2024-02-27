using Avalonia.Controls;

namespace MCU_CAN_AV.Views
{
    public partial class Control2View : UserControl
    {
        public Control2View()
        {
            InitializeComponent();

            Pnl_main.SizeChanged += (_,__) => {

                if (__.NewSize.Width < 300 || __.NewSize.Height < 300) {
                    Pnl_main2.IsVisible = false;
                }
                else if (__.NewSize.Width < 450 || __.NewSize.Height < 450)
                {
                    Pnl_main2.IsVisible = true;
                    PieChart2.IsVisible = false;
                    PieChart3.IsVisible = false;
                    PieChart2_value.Padding = new Avalonia.Thickness(0);
                    PieChart3_value.Padding = new Avalonia.Thickness(0);
                    PieChart2_value.FontSize = 30;
                    PieChart3_value.FontSize = 30;
                }
                else {
                    Pnl_main2.IsVisible = true;
                    PieChart2.IsVisible = true;
                    PieChart3.IsVisible = true;
                    PieChart2_value.Padding = new Avalonia.Thickness(0,150,0, 0);
                    PieChart3_value.Padding = new Avalonia.Thickness(0, 150, 0, 0);
                    PieChart2_value.FontSize = 30;
                    PieChart3_value.FontSize = 30;
                }
            };

        }
    }
}
