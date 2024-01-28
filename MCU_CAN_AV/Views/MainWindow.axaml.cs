using Avalonia.Controls;

namespace MCU_CAN_AV.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Width = 800;
        //mainView.SizeChanged += (_, __) =>
        //{
        //    Height = mainView.Height;
        //    Width = mainView.Width;
        //};
    }
}
