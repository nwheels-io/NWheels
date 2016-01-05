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
using NWheels.Conventions.Core;
using NWheels.Entities.Core;
using NWheels.Logging.Core;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Stacks.MongoDb.Logging;

namespace NWheels.Stacks.MongoDb
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MongoEntityObjectFactory>().As<MongoEntityObjectFactory, EntityObjectFactory, IEntityObjectFactory>() .SingleInstance();
            builder.RegisterType<MongoDataRepositoryFactory>().As<DataRepositoryFactoryBase, IDataRepositoryFactory, IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<MongoDatabaseInitializer>().As<IStorageInitializer>().SingleInstance();
            builder.RegisterType<AutoIncrementIntegerIdGenerator>().AsSelf().SingleInstance();
            builder.NWheelsFeatures().Entities().UseDefaultIdsOfType<ObjectId>();
            builder.NWheelsFeatures().Logging().RegisterLogger<IMongoDbLogger>();

            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<MongoDbThreadLogPersistor>().As<IThreadPostMortem>();
        }
    }
}
