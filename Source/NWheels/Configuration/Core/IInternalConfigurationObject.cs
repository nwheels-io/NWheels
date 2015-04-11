using System.Collections.Generic;
using NWheels.Configuration;
using System.Xml.Linq;

namespace NWheels.Core.Configuration
{
    internal interface IInternalConfigurationObject : IConfigurationObject
    {
        void LoadObject(XElement xml);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal static class ConfgurationElementExtensions
    {
        public static IInternalConfigurationObject AsInternalConfigurationObject(this IConfigurationElement element)
        {
            return (IInternalConfigurationObject)element;
        }
    }
}
