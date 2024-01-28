using Avalonia.Controls;

using Avalonia;
using MCU_CAN_AV.CustomControls;
using System.Collections.Generic;
using SkiaSharp;

namespace MCU_CAN_AV.Views
{
    public partial class MainView2 : UserControl
    {



        public MainView2()
        {
            InitializeComponent();
            setProfiView();
        }

        private void Menuitem_view_Controls_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Panel_cntrl.IsVisible = !Panel_cntrl.IsVisible;
        }

        private void Menuitem_view_Tables_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Panel_tbl.IsVisible = !Panel_tbl.IsVisible;

            if (Panel_tbl.IsVisible)
            {
                Grid_centre.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                Grid_centre.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
            }
        }
        private void Menuitem_view_Console_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Panel_console.IsVisible = !Panel_console.IsVisible;
        }
        private void Menuitem_view_Profi_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

            setProfiView();
        }
        private void Menuitem_view_User_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            setUserView();
        }

        void setProfiView() {
            Panel_console.IsVisible = true;
            Panel_cntrl.IsVisible = true;
            Panel_tbl.IsVisible = true;
            Grid_centre.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

        }

        void setUserView()
        {
            Panel_console.IsVisible = false;
            Panel_cntrl.IsVisible = true;
            Panel_tbl.IsVisible = false;
            Grid_centre.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
          
        }
    }
}
