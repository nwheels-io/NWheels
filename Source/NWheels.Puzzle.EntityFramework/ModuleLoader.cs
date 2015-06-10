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
using NWheels.Conventions.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Puzzle.EntityFramework.Conventions;
using NWheels.Puzzle.EntityFramework.Impl;

namespace NWheels.Puzzle.EntityFramework
{
    public class ModuleLoader : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EntityObjectFactory>().SingleInstance();
            builder.RegisterType<EfDataRepositoryFactory>().As<IDataRepositoryFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterInstance(SqlClientFactory.Instance).As<DbProviderFactory>();

            builder.NWheelsFeatures().Logging().RegisterLogger<IDbCommandLogger>();
            builder.RegisterType<EfLoggingDbCommandInterceptor>().As<ILifecycleEventListener>().SingleInstance();
        }
    }
}
