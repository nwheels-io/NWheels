using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;

namespace NWheels.Entities
{
    [ConfigurationSection(XmlName = "Framework.Database")]
    public interface IFrameworkDatabaseConfig : IConfigurationSection
    {
        string ConnectionString { get; set; }
    }
}
