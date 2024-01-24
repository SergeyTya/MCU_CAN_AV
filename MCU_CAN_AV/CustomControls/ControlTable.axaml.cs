using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DynamicData;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Linq;

namespace MCU_CAN_AV.CustomControls
{
    public partial class ControlTable : UserControl
    {

        public class Parameter
        {
            public enum Type{ 
                BUTTON = 0,
                LIST,
                TEXT
            };
            public Parameter(int id, string descript, Type type)
            {
                Id = id;
                Description = descript;
                this.type = type;
            }

            public int Id { get; set; }
            public string Description { get; set; }

           public Type type { get; set; }

        }

        public static readonly StyledProperty<ObservableCollection<Parameter>> TableSourceProperty =
             AvaloniaProperty.Register<ControlTable, ObservableCollection<Parameter>>("TableSource");
        public ObservableCollection<Parameter> TableSource
        {
            set => SetValue(TableSourceProperty, value);
            get => GetValue(TableSourceProperty);
        }

        public ControlTable()
        {
            InitializeComponent();
            Init_table();


          //  TableSource.CollectionChanged += TableSource_CollectionChanged;



        }
        int row_cnt = 1;
        private void TableSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tmp = (ObservableCollection<Parameter>)sender;
            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    Add_row(tmp[tmp.Count-1], row_cnt++);
                    Debug.WriteLine("sdas");
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    tmp.Clear();
                    row_cnt = 1;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        string[] headers = { "Id", "Description", "Value" };
        int[] headers_width = { 30, 140, 70 };

        void Init_table() {
            Grid_main.ColumnDefinitions.Clear();
            Grid_main.RowDefinitions.Clear();
            Grid_main.Children.Clear(); 

            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.13, GridUnitType.Star)));
            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.53, GridUnitType.Star)));
            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.33, GridUnitType.Star)));
            Grid_main.RowDefinitions.Add(new RowDefinition(new GridLength(0.13, GridUnitType.Auto)));


            for (int i = 0; i < 3; i++)
            {
                TextBlock header = new TextBlock();
                header.Text = headers[i];
                header.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                header.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                header.Width = headers_width[i];
                header.SetValue(Avalonia.Controls.Grid.ColumnProperty, i);
                header.SetValue(Avalonia.Controls.Grid.RowProperty, 0);
                Grid_main.Children.Add(header);
                Add_border(0, i);
            }
        }

        void Add_border(int row, int collumn) {
            Border bdr = new Border();
            bdr.BorderThickness = new Thickness(1);
            bdr.BorderBrush = new SolidColorBrush(Colors.White);
            bdr.SetValue(Avalonia.Controls.Grid.ColumnProperty, collumn);
            bdr.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            Grid_main.Children.Add(bdr);

        }

        void Add_row(Parameter param,int row) {
            Grid_main.RowDefinitions.Add(new RowDefinition(new GridLength(0.13, GridUnitType.Auto)));
            TextBlock id = new TextBlock();
            id.Text = param.Id.ToString();
            id.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            id.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            id.Width = headers_width[0];
            id.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
            id.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            Grid_main.Children.Add(id);
            Add_border(row, 0);
            TextBlock desc = new TextBlock();
            desc.Text = param.Description.ToString();
            desc.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            desc.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            desc.Width = headers_width[1];
            desc.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
            desc.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            Grid_main.Children.Add(desc);
            Add_border(row, 1);

            Control temp = new Control();

            switch (param.type) {
                case Parameter.Type.BUTTON:
                    temp = new Button();
                    ((Button)temp).Content = "set";

                    break;
                case Parameter.Type.TEXT:
                    temp = new TextBox();
                    break;
                case Parameter.Type.LIST:
                    temp = new ComboBox();
                    ((ComboBox)temp).Items.Add("1");
                    ((ComboBox)temp).Items.Add("2");
                    break;
                default:
                    break;
            }

            temp.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);
            temp.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            temp.Width = headers_width[2];
            Grid_main.Children.Add(temp);
            Add_border(row, 2);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            var new_value = change.NewValue;

            if (change.Property.Name == "TableSource")
            {
                // Bind data to dataGrid from property
                if (Table.ItemsSource != TableSource)
                {
                   Table.ItemsSource = TableSource;
                   TableSource.CollectionChanged += TableSource_CollectionChanged;
                }
            }



            base.OnPropertyChanged(change);
        }


    }
}
