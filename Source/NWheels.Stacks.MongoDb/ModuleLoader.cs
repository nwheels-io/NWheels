using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Conventions;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Hosting;
using MongoDB.Bson;
using NWheels.Entities.Core;
using NWheels.Stacks.MongoDb.Factories;

namespace NWheels.Stacks.MongoDb
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MongoEntityObjectFactory>().SingleInstance();
            builder.RegisterType<MongoDataRepositoryFactory>().As<IDataRepositoryFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<MongoDatabaseInitializer>().As<IStorageInitializer>().SingleInstance();
            builder.NWheelsFeatures().Entities().UseDefaultIdsOfType<ObjectId>();
            //TODO: add logging component
        }
    }
}
