using System;
using System.Text;
using System.Windows.Data;

namespace NWheels.Tools.LogViewer.Converters
{
	public class MoneyStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ( value == null )
			{
				return null;
			}

			if ( value is int && targetType == typeof(string) )
			{
				var integerValue = (int)value;
				var parameterString = ((parameter as string) ?? "I.d");
				var formatString = GetFormatString(parameterString.ToUpper(), culture);
				var forcedSign = (integerValue > 0 ? "+" : "");
				
				var stringValue = string.Format(
					culture,
					formatString,
					forcedSign,
					integerValue / 100,
					culture.NumberFormat.NumberDecimalSeparator,
					Math.Abs(integerValue % 100));

				return stringValue;
			}
			else
			{
				throw new NotSupportedException("MoneyConverter");
			}
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException("MoneyConverter");
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		private string GetFormatString(string parameterString, System.Globalization.CultureInfo culture)
		{
			var includeSign = parameterString.Contains("+");
			var includeIntegral = parameterString.Contains("I");
			var includeSeparator = parameterString.Contains(".");
			var includeDecimal = parameterString.Contains("D");

			var formatString = new StringBuilder();

			if ( includeSign )
			{
				formatString.Append("{0}");
			}

			if ( includeIntegral )
			{
				formatString.Append("{1:#,##0}");
			}

			if ( includeSeparator )
			{
				formatString.Append("{2}");
			}

			if ( includeDecimal )
			{
				formatString.Append("{3:00}");
			}

			return formatString.ToString();
		}
	}
}
