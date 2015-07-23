using System;
using System.Windows;
using System.Windows.Data;

namespace NWheels.Tools.TestBoard.Converters
{
	public class ThicknessBindingConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ( value == null )
			{
				return new Thickness(0);
			}

			var stringParameter = (parameter as string);

			if ( stringParameter != null )
			{
				var thicknessString = stringParameter.Replace("X", value.ToString());
				var innerConverter = new ThicknessConverter();
				return innerConverter.ConvertFrom(thicknessString);
			}
			else if ( value is double )
			{
				return new Thickness((double)value, 0, 0, 0);
			}
			else if ( value is string )
			{
				return new Thickness(Double.Parse((string)value), 0, 0, 0);
			}
			else
			{
				throw new NotSupportedException("MarginConverter");
			}
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("MarginConverter");
		}

		#endregion
	}
}
