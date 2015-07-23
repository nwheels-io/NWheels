using System;
using System.Windows.Data;
using NWheels.Extensions;

namespace NWheels.Tools.TestBoard.Converters
{
	public class EnumStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ( value != null && value.GetType().IsEnum && targetType.IsAssignableFrom(typeof(string)) )
			{
				return value.ToString().SplitPascalCase();
			}

			throw new NotSupportedException("EnumStringConverter");
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("EnumStringConverter");
		}
	}
}
