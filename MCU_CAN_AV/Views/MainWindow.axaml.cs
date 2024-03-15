using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MCU_CAN_AV.ViewModels;
using System;
using System.Linq;
using System.Windows.Input;

namespace MCU_CAN_AV.Views;

public partial class MainWindow : Window 
{

    /// <summary>
    /// Identifies the <seealso cref="CommandProperty"/> avalonia attached property.
    /// </summary>
    /// <value>Provide an <see cref="ICommand"/> derived object or binding.</value>
    public static readonly AttachedProperty<ICommand> CommandProperty = AvaloniaProperty.RegisterAttached<MainWindow, Interactive, ICommand>(
        "Command", default(ICommand), false, BindingMode.OneTime);

    /// <summary>
    /// Identifies the <seealso cref="CommandParameterProperty"/> avalonia attached property.
    /// Use this as the parameter for the <see cref="CommandProperty"/>.
    /// </summary>
    /// <value>Any value of type <see cref="object"/>.</value>
    public static readonly AttachedProperty<object> CommandParameterProperty = AvaloniaProperty.RegisterAttached<MainWindow, Interactive, object>(
        "CommandParameter", default(object), false, BindingMode.OneWay, null);


    public MainWindow()
    {
        InitializeComponent();
        this.Width = 800;

        this.Closing += (_,__) => {

            if(this.DataContext == null) return;
            MainViewModel? tmp = (MainViewModel)(this.DataContext);
            tmp.Closing();
        };

    }



}
