using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization.Core
{
    public interface ICoreLocalizationProvider
    {
        ICoreLocale GetCoreLocale(CultureInfo culture);
        void Refresh();
    }
}
