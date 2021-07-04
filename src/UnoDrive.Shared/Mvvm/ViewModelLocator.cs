using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;

namespace UnoDrive.Mvvm
{
    public class ViewModelLocator
    {
        public static DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached("AutoWireViewModel", typeof(bool),
            typeof(ViewModelLocator), new PropertyMetadata(false, AutoWireViewModelChanged));

        public static bool GetAutoWireViewModel(UIElement element) =>
            (bool)element.GetValue(AutoWireViewModelProperty);

        public static void SetAutoWireViewModel(UIElement element, bool value) =>
            element.SetValue(AutoWireViewModelProperty, value);

        static void AutoWireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                Bind(d);
        }

        static void Bind(DependencyObject view)
        {
            if (view is FrameworkElement frameworkElement)
            {
                var viewModelType = FindViewModel(frameworkElement.GetType());
                frameworkElement.DataContext = ActivatorUtilities.GetServiceOrCreateInstance(((App)App.Current).Container, viewModelType);
            }
        }

        private static Type FindViewModel(Type viewType)
        {
            string viewName = string.Empty;

            if (viewType.FullName.EndsWith("Page") || viewType.FullName.StartsWith("UnoDrive.Views"))
            {
                viewName = viewType.FullName
                .Replace("Page", string.Empty)
                .Replace("Views", "ViewModels");
            }

            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);

            return Type.GetType(viewModelName);
        }
    }
}
