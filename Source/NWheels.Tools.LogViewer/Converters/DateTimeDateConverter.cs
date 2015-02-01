using System;
using System.Windows.Data;

namespace NWheels.Tools.LogViewer.Converters
{
	public class DateTimeDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ( value is DateTime )
			{
				return ((DateTime)value).Date;
			}
			else
			{
				throw new NotSupportedException("DateTimeDateConverter");
			}
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("DateTimeDateConverter");
		}
	}
}
