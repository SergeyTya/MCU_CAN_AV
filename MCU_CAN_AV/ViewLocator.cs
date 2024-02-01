using Avalonia.Controls.Templates;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using MCU_CAN_AV.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCU_CAN_AV.ViewModels;
using MCU_CAN_AV.Views;

namespace MCU_CAN_AV
{
    public class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<Type, Func<Control?>> _locator = new();

        public ViewLocator()
        {
          

        }

        public Control Build(object? data)
        {
            if (data is null)
            {
                return new TextBlock { Text = "No VM provided" };
            }

            _locator.TryGetValue(data.GetType(), out var factory);

            return factory?.Invoke() ?? new TextBlock { Text = $"VM Not Registered: {data.GetType()}" };
        }

        public bool Match(object? data)
        {
            return data is ObservableObject;
        }

        private void RegisterViewFactory<TViewModel, TView>()
            where TViewModel : class
            where TView : Control
            => _locator.Add(
                typeof(TViewModel),
                Design.IsDesignMode
                    ? Activator.CreateInstance<TView>
                    : Ioc.Default.GetService<TView>);
    }
}
