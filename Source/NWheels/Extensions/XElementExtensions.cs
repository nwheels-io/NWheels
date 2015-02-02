using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NWheels.Extensions
{
    public static class XElementExtensions
    {
        public static bool NameEqualsIgnoreCase(this XElement element, string name)
        {
            return (element.Name.ToString().EqualsIgnoreCase(name));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetAttributeIgnoreCase(this XElement element, string name)
        {
            var attribute = element.Attributes().FirstOrDefault(a => a.Name.ToString().EqualsIgnoreCase(name));

            if ( attribute != null )
            {
                return attribute.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
