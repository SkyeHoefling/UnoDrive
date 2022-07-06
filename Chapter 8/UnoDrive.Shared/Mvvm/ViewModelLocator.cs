using System;
using System.Globalization;
using System.Reflection;
using Microsoft.UI.Xaml;

namespace UnoDrive.Mvvm
{
	public class ViewModelLocator
	{
		public static DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached(
			"AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), new PropertyMetadata(false, AutoWireViewModelChanged));

		public static bool GetAutoWireViewModel(UIElement element) =>
			(bool)element.GetValue(AutoWireViewModelProperty);

		public static void SetAutoWireViewModel(UIElement element, bool value) =>
			element.SetValue(AutoWireViewModelProperty, value);

		static void AutoWireViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			if ((bool)args.NewValue)
			{
				Bind(dependencyObject);
			}
		}

		static void Bind(DependencyObject view)
		{
			if (view is FrameworkElement frameworkElement)
			{
				Type viewModelType = FindViewModel(frameworkElement.GetType());

				// NOTE - This uses reflection and can cause linking issues
				frameworkElement.DataContext = Activator.CreateInstance(viewModelType);
			}
		}

		static Type FindViewModel(Type viewType)
		{
			// NOTE - This method uses reflection to access the ViewModel by convention.
			// When compiling for release mode you may have objects removed by the linker.

			string viewName = string.Empty;

			// NOTE - Some views don't use the suffix of "Page" such as the "Dashboard".
			if (viewType.FullName.EndsWith("Page") || viewType.FullName.StartsWith("UnoDrive.Views"))
			{
				viewName = viewType.FullName
					.Replace("Page", string.Empty)
					.Replace("Views", "ViewModels");
			}

			string viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;

			// NOTE - Always add the "ViewModel" suffix here which will handle the case where
			// the page doesn't have the suffix of "Page"
			string viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);

			return Type.GetType(viewModelName);
		}
    }
}
