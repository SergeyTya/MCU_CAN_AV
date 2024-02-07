using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using DynamicData;
using MCU_CAN_AV.Devices;
using MCU_CAN_AV.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Reactive.Linq;

namespace MCU_CAN_AV.CustomControls
{
    public partial class IdicatorsTable : UserControl
    {

        public static readonly StyledProperty<ObservableCollection<IDeviceParameter>> TableSourceProperty =
           AvaloniaProperty.Register<IdicatorsTable, ObservableCollection<IDeviceParameter>>("TableSource");

        List<Logger2Window> LoggerWindowsList = new();

        public ObservableCollection<IDeviceParameter> TableSource
        {
            set
            {
                TableSource_CollectionChanged(value, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                SetValue(TableSourceProperty, value);
                if (TableSource != null) TableSource.CollectionChanged += TableSource_CollectionChanged;
            }
            get => GetValue(TableSourceProperty);
        }


        public IdicatorsTable()
        {
            InitializeComponent();
            GridMain.RowDefinitions.Clear();
        }

        int row_cnt = 0;
        public void TableSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tmp = (ObservableCollection<IDeviceParameter>)sender;

            if (tmp == null)
            {
                return;
                throw new NotImplementedException();
            }

            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    IDeviceParameter item = tmp[tmp.Count - 1];
                    if (!item.IsReadWrite)
                    {
                        Add_row(item);
                    }
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
                    row_cnt = 0;
                    foreach (var el in tmp)
                    {
                        if (!el.IsReadWrite)
                        {
                             Add_row(el);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

       

        private void Add_row(IDeviceParameter item)
        {

            StackPanel spnl = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = new Thickness(5)

            };



            TextBlock Label = new TextBlock
            {
                Text = item.Name,
                TextAlignment = Avalonia.Media.TextAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Thickness(5, 5, 0, 0),
            };

            spnl.Children.Add(Label);

            if (
                   item.Options != null
                && item.Options.Count > 0
            )
            {
                TextBlock Val_op = new()
                {
                    [!TextBlock.TextProperty] = item.Value.Select(
                        (_) =>
                        {
                            int ind = (int)_;
                            string ret_Val = "";
                            if (ind >= 0 && ind < item.Options.Count)
                            {
                                ret_Val = item.Options[ind];
                            }
                            return ret_Val;
                        }).ToBinding(),
                    TextAlignment = Avalonia.Media.TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Avalonia.Media.Colors.Gray),
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                };
                spnl.Children.Add(Val_op);
            }
            else
            {
                TextBlock Value = new()
                {
                    [!TextBlock.TextProperty] = item.Value.Select(x => x.ToString("0.0#")).ToBinding(),
                    [!TextBlock.ForegroundProperty] = item.Value.Select(
                                (_) =>
                                {
                                    var ret_val = new SolidColorBrush(Avalonia.Media.Colors.Gray);


                                    ret_val = _ > item.Max ? new SolidColorBrush(Avalonia.Media.Colors.Red) :
                                    _ < item.Min ? new SolidColorBrush(Avalonia.Media.Colors.Red) : ret_val;

                                    return ret_val;
                                }).ToBinding(),
                    TextAlignment = Avalonia.Media.TextAlignment.Right,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(0, 0, 5, 0),
                };

                TextBlock Unit = new()
                {
                    Text = item.Unit,
                    TextAlignment = Avalonia.Media.TextAlignment.Right,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                };

                Grid Grid_Val = new()
                {
                    ColumnDefinitions = { new ColumnDefinition(100, GridUnitType.Star), new ColumnDefinition(100, GridUnitType.Auto) },
                    RowDefinitions = { new RowDefinition(100, GridUnitType.Star) },
                    Margin = new Thickness(5, 0, 5, 0),
                    Width = 120,
                };

                Value.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                Unit.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                Grid_Val.Children.Add(Value);
                Grid_Val.Children.Add(Unit);

                spnl.Children.Add(Grid_Val);
            }


            Border bdr = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.DimGray),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5, 5, 5, 1),
            };


            Panel pn = new();
            pn.Children.Add(spnl);
            pn.Children.Add(bdr);

            pn.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
            pn.SetValue(Avalonia.Controls.Grid.RowProperty, row_cnt++);


            spnl.Tapped += (_, _) =>
            {
                foreach (var scp in LoggerWindowsList)
                {
                    if (scp.Name == item.Name)
                    {
                        if (scp.Is_Alive)
                        {
                            scp.WindowState = WindowState.Minimized;
                            scp.WindowState = WindowState.Normal;
                            return;
                        }
                        else
                        {
                            LoggerWindowsList.Remove(scp);
                            break;
                        }
                    }
                }

                Logger2Window LoggerWindow = new Logger2Window(item.Name)
                {
                    [!Logger2Window.InputValueProperty] = item.Value.Select(x => x.ToString("0.0#")).ToBinding()

                };
                LoggerWindowsList.Add(LoggerWindow);
                LoggerWindow.Show();
            };

            GridMain.RowDefinitions.Add(new RowDefinition(new GridLength(40, GridUnitType.Auto)));
            GridMain.Children.Add(pn);
        }
    }
}
