using System;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Configuration.Core
{
    public interface IConfigurationLogger : IApplicationEventLogger
    {
        [LogActivity]
        ILogActivity LoadingConfiguration();

        [LogError]
        XmlConfigurationException SectionNotRegistered(string name);

        [LogError]
        XmlConfigurationException EveryScopeMustHaveComment(int minLength);

        [LogError]
        void BadPropertyValue(string configPath, string propertyName, Exception error);

        [LogActivity]
        ILogActivity LoadingConfigurationFile(string path);

        [LogError]
        void FailedToLoadConfigurationFile(string path, Exception error);

        [LogVerbose]
        void OptionalFileNotPresentSkipping(string path);

        [LogDebug]
        void EvaluatingXmlIfScope(string comment, bool matched);

        [LogDebug]
        void ApplyingXmlScope(string comment);

        [LogDebug]
        void ApplyingXmlConfigSection(string xmlName);
    }
}
