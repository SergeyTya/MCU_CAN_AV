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

            var services = new ServiceCollection();

            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));

            // https://jamilgeor.com/handling-errors-with-xamarin-forms-and-reactiveui/
            // https://habr.com/ru/articles/457164/

            var logger = new LogService() { Level = LogLevel.Debug };

            Locator.CurrentMutable.RegisterConstant((ILogger)     logger, typeof(ILogger));
            Locator.CurrentMutable.RegisterConstant((ILogService) logger, typeof(ILogService));

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
