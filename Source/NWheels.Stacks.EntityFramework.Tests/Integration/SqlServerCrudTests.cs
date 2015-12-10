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
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Stacks.EntityFramework.Factories;
using NWheels.Testing;
using NWheels.Testing.Entities.Impl;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

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

        [Test]
        public void CrudBasic()
        {
            //-- Arrange

            InitializeMetadata();
            var factory = CreateDynamicDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(Framework, () => Framework.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AdvancedRetrievals()
        {
            //-- Arrange

            InitializeMetadata();
            var factory = CreateDynamicDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(Framework, () => Framework.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CrudBasicWithDomainObjects()
        {
            //-- Arrange

            InitializeMetadata(concretizationsFactory: () => new[] { 
                new ConcretizationRegistration(typeof(IR1.ICustomer), typeof(IR1.ICustomer), typeof(IR1.Customer))
            });

            var factory = CreateDynamicDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteBasic(Framework, () => Framework.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AdvancedRetrievalsWithDomainObjects()
        {
            //-- Arrange

            //Framework.UpdateComponents(builder => builder.NWheelsFeatures().ObjectContracts().Concretize<IR1.ICustomer>().With<IR1.Customer>());
            //Framework.RebuildMetadataCache();

            InitializeMetadata(concretizationsFactory: () => new[] { 
                new ConcretizationRegistration(typeof(IR1.ICustomer), typeof(IR1.ICustomer), typeof(IR1.Customer))
            });
            
            var factory = CreateDynamicDataRepositoryFactory();

            DropAndCreateTestDatabase();
            InitializeCompiledModel(factory);
            CreateTestDatabaseObjects();

            //-- Act & Assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(Framework, () => Framework.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TestFixtureWithoutNodeHosts

        protected override DynamicModule CreateDynamicModule()
        {
            return _dynamicModule;
        }

        #endregion


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeMetadata(Func<ConcretizationRegistration[]> concretizationsFactory = null)
        {
            //Framework.UpdateComponents(builder => {
            //    //builder.NWheelsFeatures().ObjectContracts().Concretize<IR1.ICustomer>().With<IR1.Customer>();
            //    builder.NWheelsFeatures().Entities().UseDefaultIdsOfType<int>();
            //    builder.RegisterType<TestIdMetadataConvention>().As<IMetadataConvention>();
            //});
            //Framework.RebuildMetadataCache();

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(
                Framework.Components,
                new IMetadataConvention[] {
                    new DefaultIdMetadataConvention(typeof(int)),
                    new IntIdGeneratorMetadataConvention(), 
                },
                concretizationRegistrations: (
                    concretizationsFactory != null 
                    ? concretizationsFactory()
                    : null
                )
            );

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

            Framework.UpdateComponents(builder => {
                builder.RegisterModule<NWheels.Stacks.EntityFramework.ModuleLoader>();
            });

            return new EfDataRepositoryFactory(
                Framework.Components,
                _dynamicModule,
                new EfEntityObjectFactory(Framework.Components, _dynamicModule, (TypeMetadataCache)Framework.MetadataCache),
                (TypeMetadataCache)Framework.MetadataCache,
                new IDatabaseNameResolver[0],
                dbProviderFactory,
                Auto.Of(dbConfig));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InitializeCompiledModel(DataRepositoryFactoryBase factory)
        {
            var connection = base.CreateDbConnection();
            var repo = Framework.NewUnitOfWork<Interfaces.Repository1.IOnlineStoreRepository>(autoCommit: false);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            repo.Dispose();
        }
    }
}

