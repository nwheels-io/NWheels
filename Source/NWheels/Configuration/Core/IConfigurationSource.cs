using System.Collections.Generic;
using System.Xml.Linq;
using Autofac;

namespace NWheels.Configuration.Core
{
    public interface IConfigurationSource
    {
        IEnumerable<ConfigurationDocument> GetConfigurationDocuments();
        void ApplyConfigurationProgrammatically(IComponentContext components);
        string SourceType { get; }
        ConfigurationSourceLevel SourceLevel { get; }
    }
}
