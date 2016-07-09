using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization.Core
{
    public interface ICoreLocale
    {
        Dictionary<string, string> GetAllTranslations(IEnumerable<LocaleEntryKey> keys, bool includeOriginFallbacks = true);
    }
}
