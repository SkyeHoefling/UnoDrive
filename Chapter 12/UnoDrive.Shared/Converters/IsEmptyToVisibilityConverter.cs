using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace UnoDrive.Converters
{
	public class IsEmptyToVisibilityConverter : IValueConverter
    {
		public Visibility IsEmpty { get; set; }
		public Visibility IsNotEmpty { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is string message)
			{
				return string.IsNullOrEmpty(message) ? IsEmpty : IsNotEmpty;
			}
			else
			{
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) =>
			throw new NotSupportedException();
	}
}
