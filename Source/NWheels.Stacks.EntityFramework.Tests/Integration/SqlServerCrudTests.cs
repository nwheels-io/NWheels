using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Stacks.EntityFramework.Factories;
using NWheels.Testing;
using NWheels.Testing.Entities.Impl;
using NWheels.Testing.Entities.Stacks;

namespace NWheels.Stacks.EntityFramework.Tests.Integration
{
    [TestFixture, Category(TestCategory.Integration)]
    public class SqlServerCrudTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dynamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dynamicModule = new DynamicModule(
                "EmittedBySqlServerCrudTests",
                allowSave: true,
                saveDirectory: TestContext.CurrentContext.TestDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _dynamicModule.SaveAssembly();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void HardCodedImplementation_CrudBasic()
        {
            //-- Arrange

            InitializeMetadata();
            var factory = CreateHardCodedDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(() => factory.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void HardCodedImplementation_AdvancedRetrievals()
        {
            //-- Arrange

            InitializeMetadata();
            var factory = CreateHardCodedDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(() => factory.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void DynamicImplementation_CrudBasic()
        {
            //-- Arrange

            InitializeMetadata();
            var factory = CreateDynamicDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(() => factory.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void DynamicImplementation_AdvancedRetrievals()
        {
            //-- Arrange

            InitializeMetadata();
            var factory = CreateDynamicDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(() => factory.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TestFixtureWithoutNodeHosts

        protected override DynamicModule CreateDynamicModule()
        {
            return _dynamicModule;
        }

        #endregion


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeMetadata()
        {
            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(new IMetadataConvention[] {
                new DefaultIdMetadataConvention(typeof(int)),
                new TestIdMetadataConvention(), 
            });

            var updater = new ContainerBuilder();
            updater.RegisterInstance(metadataCache).As<ITypeMetadataCache, TypeMetadataCache>();
            updater.Update(Framework.Components.ComponentRegistry);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DataRepositoryFactoryBase CreateDynamicDataRepositoryFactory()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["test"];
            var dbProviderName = connectionString.ProviderName;
            var dbProviderFactory = DbProviderFactories.GetFactory(dbProviderName);
            var dbConfig = Framework.ConfigSection<IFrameworkDatabaseConfig>();
            dbConfig.ConnectionString = connectionString.ConnectionString;

            return new EfDataRepositoryFactory(
                Framework.Components,
                _dynamicModule,
                new EfEntityObjectFactory(Framework.Components, _dynamicModule, (TypeMetadataCache)Framework.MetadataCache),
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
                _dynamicModule,
                (TypeMetadataCache)Framework.MetadataCache,
                connectionString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeCompiledModel(DataRepositoryFactoryBase factory)
        {
            var connection = base.CreateDbConnection();
            var repo = factory.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            repo.Dispose();
        }
    }
}
