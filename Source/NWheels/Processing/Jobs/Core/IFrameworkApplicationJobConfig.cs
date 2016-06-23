using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;

namespace NWheels.Processing.Jobs.Core
{
    [ConfigurationSection(XmlName = "Framework.Jobs")]
    public interface IFrameworkApplicationJobConfig : IConfigurationSection
    {
        string JobsInBatch { get; set; }
    }
}
