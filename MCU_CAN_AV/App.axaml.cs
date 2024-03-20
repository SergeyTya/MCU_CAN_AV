using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using MCU_CAN_AV.utils;
using MCU_CAN_AV.ViewModels;
using MCU_CAN_AV.Views;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using System;
using Splat.Serilog;
using Serilog;
using ReactiveUI;
using System.Reactive.Linq;
using System.Globalization;

namespace MCU_CAN_AV;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            //var services = new ServiceCollection();

            //services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));

            Locator.CurrentMutable.RegisterConstant(new FilesService(desktop.MainWindow), typeof(IFilesService) );

            //// https://jamilgeor.com/handling-errors-with-xamarin-forms-and-reactiveui/
            //// https://www.codeproject.com/Articles/5357417/LogViewer-Control-for-WinForms-WPF-and-Avalonia-in#solution-setup
            //https://libraries.io/nuget/Splat.Microsoft.Extensions.Logging/8.2.4
            //https://www.youtube.com/watch?v=nVAkSBpsuTk
            // https://habr.com/ru/articles/457164/

            Locator.CurrentMutable.RegisterConstant(new LogProvider(), typeof(ILogProvider));
            Locator.CurrentMutable.RegisterConstant(new DataLogger(), typeof(IDataLogger));

            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Observers(events => events.Do(evt =>{
                     Locator.Current.GetService<ILogProvider>()?.Post(
                     $"   {evt.Timestamp.LocalDateTime} :   [ {evt.Level} ]  {evt.MessageTemplate.Text} \n"
                         );
                 }).Subscribe())
            .CreateLogger();

            Locator.CurrentMutable.UseSerilogFullLogger();

        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

        }

        base.OnFrameworkInitializationCompleted();


    }

    public new static App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

}
