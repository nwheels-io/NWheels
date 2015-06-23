using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.UI.Core
{
    public interface IUILocalizationProvider
    {
        Dictionary<string, string> GetLocalStrings(IEnumerable<string> stringIds, CultureInfo culture);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class VoidUILocalizationProvider : IUILocalizationProvider
    {
        public Dictionary<string, string> GetLocalStrings(IEnumerable<string> stringIds, CultureInfo culture)
        {
            return stringIds.ToDictionary(s => s, s => s.SplitPascalCase());
        }
    }
}
