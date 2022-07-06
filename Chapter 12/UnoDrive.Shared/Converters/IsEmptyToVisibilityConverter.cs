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
			var message = (string)value;
			return string.IsNullOrEmpty(message) ? IsEmpty : IsNotEmpty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
	}
}
