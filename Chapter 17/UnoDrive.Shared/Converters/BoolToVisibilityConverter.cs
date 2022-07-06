using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace UnoDrive.Converters
{
	public class BoolToVisibilityConverter : IValueConverter
    {
		public Visibility TrueValue { get; set; }
		public Visibility FalseValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			bool boolValue = value is bool && (bool)value;
			return boolValue ? TrueValue : FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language) =>
			throw new NotSupportedException();
	}
}
