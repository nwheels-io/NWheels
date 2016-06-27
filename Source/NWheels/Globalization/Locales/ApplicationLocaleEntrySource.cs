using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Globalization.Locales
{
    public class ApplicationLocaleEntrySource
    {
        public ApplicationLocaleEntrySource(Type uidlApplicationType)
        {
            UidlApplicationType = uidlApplicationType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type UidlApplicationType { get; private set; }
    }
}
