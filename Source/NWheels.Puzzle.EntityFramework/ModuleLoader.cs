using Autofac;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NWheels.Conventions;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Puzzle.EntityFramework.Conventions;

namespace NWheels.Puzzle.EntityFramework
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EfEntityObjectFactory>().SingleInstance();
            builder.RegisterType<EfDataRepositoryFactory>().As<IDataRepositoryFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterInstance(SqlClientFactory.Instance).As<DbProviderFactory>();
        }
    }
}
