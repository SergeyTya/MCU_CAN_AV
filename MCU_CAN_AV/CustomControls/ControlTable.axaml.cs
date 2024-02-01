using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.Converters;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.DesignerSupport.Remote;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using MCU_CAN_AV.DeviceDescriprion;
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

        public static readonly StyledProperty<ObservableCollection<DeviceParameter>> TableSourceProperty =
             AvaloniaProperty.Register<ControlTable, ObservableCollection<DeviceParameter>>("TableSource");

        public ObservableCollection<DeviceParameter> TableSource
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

        public ControlTable()
        {
            InitializeComponent();
            Init_table();
        }
        int row_cnt = 1;
        private void TableSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tmp = (ObservableCollection<DeviceParameter>)sender;
            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    DeviceParameter item = tmp[tmp.Count - 1];
                    Add_row(item, row_cnt++);
                    Debug.WriteLine("sdas");
                    break;

                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
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
            bdr.BorderThickness = new Thickness(th_left, th_top, 1, 1);
            // bdr.BorderThickness = new Thickness(0, 0, 0, 0);
            bdr.BorderBrush = new SolidColorBrush(Colors.Gray);
            bdr.SetValue(Avalonia.Controls.Grid.ColumnProperty, col);
            bdr.SetValue(Avalonia.Controls.Grid.RowProperty, row);
            Parent.Children.Add(bdr);

        }

        Panel newTextblokCell(string Text, Control Parent, int row, int col)
        {


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
            ((Grid)Parent).Children.Add(panel);
            Add_border(row, col, panel);

            return panel;
        }

        string[] headers = { "Description", "Id", "Value" };


        int vl_width = 70;


        void Init_table()
        {
            Grid_main.ColumnDefinitions.Clear();
            Grid_main.RowDefinitions.Clear();
            Grid_main.Children.Clear();

            Grid_header.ColumnDefinitions.Clear();
            Grid_header.RowDefinitions.Clear();
            Grid_header.Children.Clear();

            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.0, GridUnitType.Auto)));
            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.0, GridUnitType.Star)));
            Grid_main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(vl_width, GridUnitType.Pixel)));

            Grid_header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100.0, GridUnitType.Auto)));
            Grid_header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100.0, GridUnitType.Auto)));
            Grid_header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(vl_width, GridUnitType.Auto)));

            GridSplitter split1 = new GridSplitter
            {
                ResizeBehavior = GridResizeBehavior.PreviousAndCurrent,
                ResizeDirection = GridResizeDirection.Columns,
                Background = new SolidColorBrush(Colors.DimGray),
                Height = 5,
            };

            Action resize = () =>
            {
                // TODO Fix init load column width bug
                for (Int32 i = 0; i < 3; i++)
                {
                    Grid_header.ColumnDefinitions[i].Width = Grid_main.ColumnDefinitions[i].Width;
                }
            };

            split1.DragCompleted += (s, e) =>
            {
                resize();
            };

            split1.DoubleTapped += (s, e) =>
            {
                Grid_main.ColumnDefinitions[1].Width = new GridLength(1.0, GridUnitType.Star);
                resize();
            };

            split1.SetValue(Avalonia.Controls.Grid.RowProperty, 0);
            split1.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
            Grid_main.Children.Add(split1);

            Grid_header.RowDefinitions.Add(new RowDefinition(new GridLength(30, GridUnitType.Pixel)));
            Grid_main.RowDefinitions.Add(new RowDefinition(new GridLength(0.13, GridUnitType.Auto)));

            for (int i = 0; i < 3; i++)
            {
                Add_column_header(headers[i], Grid_header, 0, i);
            }
            resize();
        }

        void Add_column_header(string Text, Grid Parent, int row, int col)
        {

            var panel = newTextblokCell(Text, Parent, row, col);
            ((TextBlock)panel.Children[0]).HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            ((TextBlock)panel.Children[0]).TextAlignment = TextAlignment.Center;

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
        string tbvaluebuf = "0";
        void Add_row(DeviceParameter param, int row)
        {


            change_color = !change_color;

            //var c1 = new Avalonia.Media.Color(100, 100, 100, 100);
            //var c2 = new Avalonia.Media.Color(80, 80, 80, 80);

            var c1 = Avalonia.Media.Colors.Gray;
            var c2 = Avalonia.Media.Colors.DimGray;

            var row_color = change_color ? c1 : c2;

            //param.row = row;

            var panel = newTextblokCell(param.CANID, Grid_main, row, 1);
            ((TextBlock)panel.Children[0]).Margin = new Thickness(1, 1, 1, 1);
            ((TextBlock)panel.Children[0]).HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            ((TextBlock)panel.Children[0]).TextAlignment = TextAlignment.Center;
            panel.Background = new SolidColorBrush(row_color);


            panel = newTextblokCell(param.sname.ToString(), Grid_main, row, 0);
            ((TextBlock)panel.Children[0]).HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            ((TextBlock)panel.Children[0]).Margin = new Thickness(0, 0, 10, 0);
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
                    Margin = new Thickness(10, 1, 1, 1),
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.Black),

                };

                return item2;
            };

            if (param.options != null)
            {

                var item1 = new ComboBox
                {
                    [!ComboBox.SelectedIndexProperty] = binding,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Foreground = new SolidColorBrush(Colors.Black),
                };

                foreach (var item in param.options)
                {
                    item1.Items.Add(item);
                }

                item1.DropDownClosed += (_, __) =>
                {
                    if (item1.SelectedIndex < 0) return;
                    param.onValueChanged.Invoke(item1.SelectedIndex, EventArgs.Empty);
                };
                temp = item1;
                temp.IsEnabled = param.RW;

            }
            else
            {

                if (!param.RW)
                {
                    temp = Create_RO_item();
                }
                else
                {

                    var tb = new TextBox
                    {
                        [!TextBox.TextProperty] = binding,
                        Margin = new Thickness(1, 1, 1, 1),
                        Foreground = new SolidColorBrush(Colors.LightGray),

                    };
                    tb.TextChanged += (_, __) =>
                    {
                        tb.Background = new SolidColorBrush(Colors.Gray);
                    };
                    Action undo = () =>
                    {
                        tb.Text = tbvaluebuf;
                    };
                    Action endedit = () =>
                    {

                        var nextElement = KeyboardNavigationHandler.GetNext(tb, NavigationDirection.Next);
                        nextElement.Focus();

                    };
                    tb.KeyDown += (_, __) =>
                    {
                        if (__.Key == Avalonia.Input.Key.Up)
                        {
                            var nextElement = KeyboardNavigationHandler.GetNext(tb, NavigationDirection.Previous);
                            nextElement.Focus();
                            return;
                        }
                        if (__.Key == Avalonia.Input.Key.Down)
                        {
                            var nextElement = KeyboardNavigationHandler.GetNext(tb, NavigationDirection.Next);
                            nextElement.Focus();
                            return;
                        }

                        if (__.Key == Avalonia.Input.Key.Escape)
                        {
                            undo();
                            var nextElement = KeyboardNavigationHandler.GetNext(Grid_main, NavigationDirection.Next);
                            nextElement.Focus();
                        }
                        if (__.Key == Avalonia.Input.Key.Enter)
                        {

                            double value = 0;
                            bool res = double.TryParse(tb.Text, out value);
                            if (!res)
                            {
                                undo();
                                endedit();
                                return;
                            }

                            if (param.onValueChanged != null)
                                param.onValueChanged.Invoke(value, EventArgs.Empty);

                            tb.Background = new SolidColorBrush(Colors.Green);
                            endedit();
                        }
                    };

                    tb.LostFocus += (_, __) =>
                    {
                        double value = 0;
                        bool res = double.TryParse(tb.Text, out value);
                        if (res)
                        {
                            if (param.onValueChanged != null)
                                param.onValueChanged.Invoke(value, EventArgs.Empty);
                        }
                        else
                        {
                            undo();
                        }
                    };

                    tb.GotFocus += (_, __) =>
                    {
                        tbvaluebuf = tb.Text;
                    };

                    temp = tb;

                }

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

            // if (change.Property.Name == "TableSource")
            if (change.Property == TableSourceProperty)
            {
                if (!TableSource_init)
                {
                    TableSource.CollectionChanged += TableSource_CollectionChanged;
                    TableSource_init = true;
                }
            }

            base.OnPropertyChanged(change);
        }


    }
}
