using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    [ConfigurationSection(XmlName = "MongoDb.ThreadLogs")]
    public interface IMongoDbThreadLogPersistorConfig : IConfigurationSection
    {
        [DefaultValue(2)]
        int ThreadCount { get; set; }

        [DefaultValue(100)]
        int BatchSize { get; set; }

        [DefaultValue("00:00:00.500")]
        TimeSpan BatchTimeout  { get; set; }

        [DefaultValue(2048L)]
        long MaxCollectionSizeMb { get; set; }

        [DefaultValue(2000000L)]
        long MaxCollectionDocuments { get; set; }
    }
}
