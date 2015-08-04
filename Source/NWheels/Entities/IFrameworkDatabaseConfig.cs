using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    [ConfigurationSection(XmlName = "Framework.Database")]
    public interface IFrameworkDatabaseConfig : IConfigurationSection
    {
        [PropertyContract.Security.Sensitive]
        string ConnectionString { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Security.Sensitive]
        string MasterConnectionString { get; set; }
    }
}
