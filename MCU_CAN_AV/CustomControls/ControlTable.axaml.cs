using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.Converters;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.DesignerSupport.Remote;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using MCU_CAN_AV.ViewModels;
using MCU_CAN_AV.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Reactive.Linq;

namespace MCU_CAN_AV.CustomControls
{
    public partial class ControlTable : UserControl
    {

        public enum ContentType
        {
            ALL = 0,
            RO,
            RW
        };

        public static readonly StyledProperty<ObservableCollection<Parameter>> TableSourceProperty =
             AvaloniaProperty.Register<ControlTable, ObservableCollection<Parameter>>("TableSource");

        public ObservableCollection<Parameter> TableSource
        {
            set => SetValue(TableSourceProperty, value);
            get => GetValue(TableSourceProperty);
        }

        public static readonly StyledProperty<ContentType> ContentTypeProperty =
          AvaloniaProperty.Register<ControlTable, ContentType>("TypeOfContent");

        public ContentType TypeOfContent
        {
            set => SetValue(ContentTypeProperty, value);
            get => GetValue(ContentTypeProperty);
        }


        public partial class Parameter : ObservableObject
        {
            public enum Type{ 
                BOOL = 0,
                LIST,
                TEXT
            };
            public Parameter(string id, string descript, Type type= Type.TEXT, List<string>? items = null, bool writeEnable = false)
            {
                Id = id;
                Description = descript;
                this.type = type;
                this.items = items;
                isWriteEnable = writeEnable;
            }

            public int row;
              
            public string Id { get; set; }
            public string Description { get; set; }

            public Type type { get; set; }

            public bool isWriteEnable;
            
            internal List<string> items;



            [ObservableProperty]
            private double _value;

            internal  EventHandler onValueChanged;
            public event EventHandler onValueChangedByUser
            {
                add
                {
                    lock (this) { onValueChanged = onValueChanged + value; }
                }
                remove
                {
                    lock (this) { onValueChanged = onValueChanged - value; }
                }
            }
        }

        public ControlTable()
        {
            InitializeComponent();
            Init_table();
        }
        int row_cnt = 1;
        private void TableSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tmp = (ObservableCollection<Parameter>)sender;
            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    Parameter item = tmp[tmp.Count - 1];
                    switch (this.TypeOfContent)
                    {
                        case ContentType.ALL:
                            break;
                        case ContentType.RO:
                            if (item.isWriteEnable) return;
                            break;
                        case ContentType.RW:
                            if (!item.isWriteEnable) return;
                            break;
                        default:
                            break;
                    }
                    Add_row(item, row_cnt++);
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

        void Add_border(int row, int col, Panel Parent)
        {
            int th_top = 1;
            int th_left = 1;
            if (row != 0) th_top = 0;
            if (col != 0) th_left = 0;
            Border bdr = new Border();
            bdr.BorderThickness = new Thickness(th_left, th_top, 1,1);
           // bdr.BorderThickness = new Thickness(0, 0, 0, 0);
            bdr.BorderBrush = new SolidColorBrush(Colors.Gray);
            bdr.SetValue(Avalonia.Controls.Grid.ColumnProperty, col);
            bdr.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            Parent.Children.Add(bdr);

        }

        Panel newTextblokCell(string Text, Control Parent , int row, int col) {


            TextBlock header = new TextBlock
            {
                Text = Text,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeight.Regular
            };
            var panel = new Panel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            panel.Children.Add(header);
            panel.SetValue(Avalonia.Controls.Grid.ColumnProperty, col);
            panel.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            ((Grid) Parent).Children.Add(panel);
            Add_border(row, col, panel);

            return panel;
        }

        string[] headers = {  "Description", "Id", "Value" };

        int id_width = 100;
        int vl_width = 100;


        void Init_table() {
            Grid_main.ColumnDefinitions.Clear();
            Grid_main.RowDefinitions.Clear();
            Grid_main.Children.Clear();

            Grid_header.ColumnDefinitions.Clear();
            Grid_header.RowDefinitions.Clear();
            Grid_header.Children.Clear();

            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.0, GridUnitType.Star)));
            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(id_width, GridUnitType.Pixel)));
            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(vl_width, GridUnitType.Pixel)));

            Grid_header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.0, GridUnitType.Star)));
            Grid_header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(id_width, GridUnitType.Pixel)));
            Grid_header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(vl_width, GridUnitType.Pixel)));


            Grid_header.RowDefinitions.Add(new RowDefinition(new GridLength(30, GridUnitType.Pixel)));
            Grid_main.RowDefinitions.Add(new RowDefinition(new GridLength(0.13, GridUnitType.Auto)));

            for (int i = 0; i < 3; i++)
            {
               Add_column_header(headers[i], Grid_header, 0, i);
               
            }
        }

        void Add_column_header(string Text,  Grid Parent, int row, int col) {
          
            var panel =  newTextblokCell(Text, Parent, row, col);

            panel.Background = new SolidColorBrush(Colors.Black);   

            TextBlock tb = (TextBlock)panel.Children[0];
            Border bdr = (Border)panel.Children[1];

            tb.Foreground = new SolidColorBrush(Colors.Gray);
            bdr.BorderBrush = new SolidColorBrush(Colors.Gray);

            int th_left = 1;
            if (col != 0) th_left = 0;
            bdr.BorderThickness = new Thickness(th_left, 0, 1, 0);

        }

        bool change_color = false;
        void Add_row(Parameter param, int row) {


            change_color = !change_color;

            //var c1 = new Avalonia.Media.Color(100, 100, 100, 100);
            //var c2 = new Avalonia.Media.Color(80, 80, 80, 80);

            var c1 = Avalonia.Media.Colors.Gray;
            var c2 = Avalonia.Media.Colors.DimGray;

            var row_color = change_color? c1 : c2;

            param.row = row;

            var panel = newTextblokCell(param.Id, Grid_main, row, 1);
            ((TextBlock)panel.Children[0]).Margin = new Thickness(1, 1, 1, 1);
            ((TextBlock)panel.Children[0]).HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            ((TextBlock)panel.Children[0]).TextAlignment = TextAlignment.Center;
            panel.Background = new SolidColorBrush(row_color);

          
            panel = newTextblokCell(param.Description.ToString(),  Grid_main, row, 0);
            ((TextBlock)panel.Children[0]).HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            ((TextBlock)panel.Children[0]).Margin = new Thickness(0,0,10,0);
            panel.Background = new SolidColorBrush(row_color);

            panel = new DockPanel();
            Control temp = new Control();

            Binding binding = new Binding
            {
                Path = "Value",
                Source = param,
                Mode = BindingMode.OneWay
            };

            Func<Control> Create_RO_item = () =>
            {
                var item2 = new TextBlock
                {
                    [!TextBlock.TextProperty] = binding,
                    Margin = new Thickness(1, 1, 1, 1),
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.Black),
                };

                return item2;
            };

            switch (param.type) {
                case Parameter.Type.BOOL:
                    
                    if(!param.isWriteEnable)
                    {
                        temp = Create_RO_item();
                        break;

                    }
                    panel = new DockPanel
                    {
                      
                    };

                    var item0 = new CheckBox {
                        [!CheckBox.IsCheckedProperty] = binding,
                        Margin = new Thickness(10,0,0,0),
                        Foreground = new SolidColorBrush(Colors.Black),

                    };

                    item0.IsCheckedChanged += (_, __) =>
                    {
                        param.onValueChanged.Invoke(item0.IsChecked, EventArgs.Empty);
                    };

                    panel.Children.Add(item0);

                    temp = panel;

                    break;

                case Parameter.Type.TEXT:

                    if (!param.isWriteEnable)
                    {
                        temp = Create_RO_item();
                        break;

                    }

                    temp = new TextBox {
                        [!TextBox.TextProperty] = binding,
                        Margin = new Thickness (1,1,1,1),
                        Foreground = new SolidColorBrush(Colors.Black),
                    };
                    temp.KeyUp += (_,__) => { // set new value

                        Action undo = () => {
                            ((TextBox)_).Undo();
                           while(true)
                           {
                                if (((TextBox)_).Text != null) { ((TextBox)_).Undo(); }
                                else {
                                ((TextBox)_).Redo();
                                break;
                             }
                           }
                        };

                        if (__.Key == Avalonia.Input.Key.Escape)
                        {
                            undo();
                        }
                        if (__.Key != Avalonia.Input.Key.Enter) return;

                        double value = 0;
                        bool res = double.TryParse(((TextBox)_).Text, out value );
                        if(!res) undo();
                        if (param.onValueChanged != null)
                            param.onValueChanged.Invoke(value, EventArgs.Empty);
                    };
                    break;

                case Parameter.Type.LIST:
                    var item1 = new ComboBox {
                        [!ComboBox.SelectedIndexProperty] = binding,
                        VerticalAlignment= Avalonia.Layout.VerticalAlignment.Stretch,
                        Foreground = new SolidColorBrush (Colors.Black),
                    };

                    foreach (var item in param.items) {
                        item1.Items.Add(item);
                    }

                    item1.DropDownClosed += (_, __) =>
                    {
                        if (item1.SelectedIndex < 0) return;
                        param.onValueChanged.Invoke(item1.SelectedIndex, EventArgs.Empty);
                    };
                    temp = item1;
                    temp.IsEnabled = param.isWriteEnable;
                    break;
                default:
                    break;
            }

          
            temp.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            panel = new Panel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };
            panel.Children.Add(temp);
            panel.Background = new SolidColorBrush(row_color);
            panel.SetValue(Avalonia.Controls.Grid.ColumnProperty, 2);
            panel.SetValue(Avalonia.Controls.Grid.RowProperty, row);

            Grid_main.RowDefinitions.Add(new RowDefinition(new GridLength(40, GridUnitType.Pixel)));
            Grid_main.Children.Add(panel);
            Add_border(row, 2, (Panel)Grid_main);
        }

        bool TableSource_init = false;
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            var new_value = change.NewValue;

            if (change.Property.Name == "TableSource")
            {
                if (!TableSource_init) {
                    TableSource.CollectionChanged += TableSource_CollectionChanged;
                    TableSource_init = true;
                } 
            }

            base.OnPropertyChanged(change);
        }


    }
}
