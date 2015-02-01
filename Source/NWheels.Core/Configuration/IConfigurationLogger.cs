using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Core.Configuration
{
    public interface IConfigurationLogger : IApplicationEventLogger
    {
        [LogError]
        void BadPropertyValue(string configPath, string propertyName, Exception error);
    }
}
