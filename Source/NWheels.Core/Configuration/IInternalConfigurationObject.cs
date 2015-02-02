using NWheels.Configuration;
using System.Xml.Linq;

namespace NWheels.Core.Configuration
{
    internal interface IInternalConfigurationObject : IConfigurationObject
    {
        void LoadObject(XElement xml);
    }
}
