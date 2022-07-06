using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace UnoDrive.Converters
{
	public class NullToVisibilityConverter : IValueConverter
    {
		public Visibility IsNull { get; set; }
		public Visibility HasValue{ get; set; }

		public object Convert(object value, Type targetType, object parameter, string language) =>
			value == null ? IsNull : HasValue;

		public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
	}
}
