using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Stacks.EntityFramework.Factories;
using NWheels.Testing;
using NWheels.Testing.Entities.Stacks;

namespace NWheels.Stacks.EntityFramework.Tests.Unit
{
    [TestFixture]
    public class EfDataRepositoryFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanCreateHardCodedDataRepository()
        {
            FactoryOperations.Repository1.ExecuteDataRepositoryCreation(Framework, CreateHardCodedDataRepositoryFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDynamicDataRepository()
        {
            FactoryOperations.Repository1.ExecuteDataRepositoryCreation(Framework, CreateDynamicDataRepositoryFactory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DataRepositoryFactoryBase CreateDynamicDataRepositoryFactory()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["test"];
            var dbProviderName = connectionString.ProviderName;
            var dbProviderFactory = DbProviderFactories.GetFactory(dbProviderName );
            var dbConfig = Framework.ConfigSection<IFrameworkDatabaseConfig>();
            dbConfig.ConnectionString = connectionString.ConnectionString;

            return new EfDataRepositoryFactory(
                Framework.Components, 
                base.DyamicModule,
                new EfEntityObjectFactory(Framework.Components, base.DyamicModule, (TypeMetadataCache)Framework.MetadataCache),
                (TypeMetadataCache)Framework.MetadataCache,
                dbProviderFactory,
                Auto.Of(dbConfig));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DataRepositoryFactoryBase CreateHardCodedDataRepositoryFactory()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["test"];
            //var dbProviderName = connectionString.ProviderName;
            //var dbProviderFactory = DbProviderFactories.GetFactory(dbProviderName);
            var dbConfig = Framework.ConfigSection<IFrameworkDatabaseConfig>();
            dbConfig.ConnectionString = connectionString.ConnectionString;

            return new HardCodedImplementations.DataRepositoryFactory_OnlineStoreRepository(
                Framework,
                base.DyamicModule,
                (TypeMetadataCache)Framework.MetadataCache);
        }
    }
}
