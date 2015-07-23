using System;
using System.Globalization;
using System.Windows.Data;

namespace NWheels.Tools.TestBoard.Converters
{
	public class TupleConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if ( values != null )
			{
				switch ( values.Length )
				{
					case 1:
						return new Tuple<object>(values[0]);
					case 2:
						return new Tuple<object, object>(values[0], values[1]);
					case 3:
						return new Tuple<object, object, object>(values[0], values[1], values[2]);
				}
			}

			throw new NotSupportedException("TupleConverter");
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			var tuple1 = (value as Tuple<object>);
			var tuple2 = (value as Tuple<object, object>);
			var tuple3 = (value as Tuple<object, object, object>);

			if ( tuple1 != null )
			{
				return new[] {tuple1.Item1};
			}
			else if ( tuple2 != null )
			{
				return new[] {tuple2.Item1, tuple2.Item2};
			}
			else if ( tuple3 != null )
			{
				return new[] {tuple3.Item1, tuple3.Item2, tuple3.Item3};
			}
			else
			{
				throw new NotSupportedException("TupleConverter");
			}
		}
	}
}
