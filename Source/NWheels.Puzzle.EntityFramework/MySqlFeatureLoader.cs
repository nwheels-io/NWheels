using Autofac;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;

namespace NWheels.Puzzle.EntityFramework
{
    public class MySqlFeatureLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new MySqlClientFactory()).As<DbProviderFactory>();
        }
    }
}
