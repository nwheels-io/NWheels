using System;
using System.Windows.Data;
using NWheels.Tools.TestBoard.Modules.LogViewer;

namespace NWheels.Tools.TestBoard.Converters
{
    public class LogNodeDetailsTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
		    if ( value == null )
		    {
		        return "(no item selected)";
		    }

			if ( value is ThreadLogViewModel.NodeItem && targetType.IsAssignableFrom(typeof(string)) )
			{
			    return ((ThreadLogViewModel.NodeItem)value).GetFullDetailsText();
			}

            throw new NotSupportedException("LogNodeDetailsTextConverter");
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            throw new NotSupportedException("LogNodeDetailsTextConverter");
		}
	}
}
