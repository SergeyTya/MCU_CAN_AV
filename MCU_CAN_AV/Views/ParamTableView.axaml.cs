using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Diagnostics;

namespace MCU_CAN_AV.Views
{
    public partial class ParamTableView : UserControl
    {
        public ParamTableView()
        {
            InitializeComponent();
        }


        void ButtonClickEvent(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Command.Execute(this); 
            DataGrid.CommitEdit();
        }

        void TextBoxKeyUpEvent(object sender, KeyEventArgs e) {

            if (e.Key == Avalonia.Input.Key.Enter)
            {
                DataGrid.CommitEdit();
            }

        }

        /*
         
        

         */
    }
}
