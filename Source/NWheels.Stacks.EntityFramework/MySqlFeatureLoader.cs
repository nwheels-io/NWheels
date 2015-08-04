using Autofac;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using NWheels.Entities.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.EntityFramework
{
    public class MySqlFeatureLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new MySqlClientFactory()).As<DbProviderFactory>();
            builder.RegisterType<MySqlSchemaInitializer>().As<IStorageInitializer>();
            builder.NWheelsFeatures().Entities().UseUnderscoreRelationalMappingConvention();
        }
    }
}
