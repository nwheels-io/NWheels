using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization.Core
{
    public interface ICoreLocale
    {
        void SetLocalStrings(Dictionary<string, string> localStringByStringId);
    }
}
