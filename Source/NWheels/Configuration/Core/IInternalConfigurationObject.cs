using System.Xml.Linq;

namespace NWheels.Configuration.Core
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
