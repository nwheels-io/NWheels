using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.DataObjects;

namespace NWheels.Processing.Jobs.Core
{
    [ConfigurationSection(XmlName = "Framework.Jobs")]
    public interface IFrameworkApplicationJobConfig : IConfigurationSection
    {
        string JobsInBatch { get; set; }
        
        INamedObjectCollection<IFrameworkSingleJobConfig> Jobs { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [ConfigurationElement(XmlName = "Job")]
    public interface IFrameworkSingleJobConfig : INamedConfigurationElement
    {
        [PropertyContract.DefaultValue("00:00:30")]
        TimeSpan PeriodicInterval { get; set; }
    }
}
