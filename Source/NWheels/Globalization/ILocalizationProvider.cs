using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NWheels.Extensions;

namespace NWheels.Globalization
{
    public interface ILocalizationProvider
    {
        Dictionary<string, string> GetLocalStrings(IEnumerable<string> stringIds, CultureInfo culture);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class VoidLocalizationProvider : ILocalizationProvider
    {
        public Dictionary<string, string> GetLocalStrings(IEnumerable<string> stringIds, CultureInfo culture)
        {
            return stringIds.ToDictionary(s => s, s => s.SplitPascalCase());
        }
    }
}
