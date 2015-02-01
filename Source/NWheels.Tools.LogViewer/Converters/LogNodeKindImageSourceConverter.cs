using System;
using System.Windows.Data;

namespace NWheels.Tools.LogViewer.Converters
{
	public class LogNodeKindImageSourceConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ( value is LogNodeKind )
			{
                switch ( (LogNodeKind)value )
				{
                    case LogNodeKind.LogDebug:
                    case LogNodeKind.LogError:
						return "pack://application:,,,/Graphics/StatusOkImage16.png";
                    case LogNodeKind.LogWarning:
						return "pack://application:,,,/Graphics/StatusAlertImage16.png";
                    case LogNodeKind.LogInfo:
						return "pack://application:,,,/Graphics/StatusFailureImage16.png";
				}
			}

            throw new NotSupportedException("LogNodeKindImageSourceConverter");
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            throw new NotSupportedException("LogNodeKindImageSourceConverter");
		}
	}
}
