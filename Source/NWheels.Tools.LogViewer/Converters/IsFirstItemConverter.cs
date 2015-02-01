using System;
using System.Linq;
using System.Windows.Data;

namespace NWheels.Tools.LogViewer.Converters
{
	public class IsFirstItemConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ( values != null && values.Length == 2 && values[1] is System.Collections.IEnumerable )
			{
				var item = values[0];
				var itemsSource = (System.Collections.IEnumerable)values[1];

				return (itemsSource.Cast<object>().FirstOrDefault() == item);
			}

			throw new NotSupportedException("IsFirstItemConverter");
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("IsFirstItemConverter");
		}
	}
}
