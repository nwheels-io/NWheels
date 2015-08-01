using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Stacks.EntityFramework.Factories;
using NWheels.Testing;
using NWheels.Testing.DataObjects;
using IR3A = NWheels.Testing.Entities.Stacks.Interfaces.Repository3A;
using IR3B = NWheels.Testing.Entities.Stacks.Interfaces.Repository3B;

namespace NWheels.Stacks.EntityFramework.ComponentTests
{
    [TestFixture, Ignore("WIP"), Category("Integration")]
    public class UserAccountDataRepositoryTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dynamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dynamicModule = new DynamicModule(
                "EmittedByUserAccountDataRepositoryTests",
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
        public void CanCreateMetadata()
        {
            //-- Arrange

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(GetRepositoryMixinsRegistrations());

            //-- Act

            var userAccountMetadata = metadataCache.GetTypeMetadata(typeof(IR3A.IUserAccountEntity));

            //-- Assert

            Console.WriteLine(Jsonlike.Stringify(userAccountMetadata));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateEntityObjects()
        {
            //-- Arrange

            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(GetRepositoryMixinsRegistrations());
            var entityObjectFactory = new EntityObjectFactory(Framework.Components, _dynamicModule, metadataCache);

            //-- Act

            var password = entityObjectFactory.NewEntity<IR3A.IPasswordEntity>();
            var role = entityObjectFactory.NewEntity<IR3A.IUserRoleEntity>();
            var userAccount = entityObjectFactory.NewEntity<IR3A.IUserAccountEntity>();

            //-- Assert

            Assert.That(password, Is.Not.Null);
            Assert.That(role, Is.Not.Null);
            Assert.That(userAccount, Is.Not.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateDataRepositoryObject()
        {
            var repoFactory = CreateDataRepositoryFactory();

            using ( var connection = base.CreateDbConnection() )
            {
                repoFactory.NewUnitOfWork<IR3B.IMyAppDataRepository>(autoCommit: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EfDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(GetRepositoryMixinsRegistrations());
            var entityFactory = new EfEntityObjectFactory(Framework.Components, _dynamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(Framework.Components, _dynamicModule, entityFactory, metadataCache, SqlClientFactory.Instance, ResolveAuto<IFrameworkDatabaseConfig>());

            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IR3B.IMyAppDataRepository InitializeDataRepository()
        {
            var repoFactory = CreateDataRepositoryFactory();

            var connection = base.CreateDbConnection();
            var repo = repoFactory.NewUnitOfWork<IR3B.IMyAppDataRepository>(autoCommit: true);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            return (IR3B.IMyAppDataRepository)repo;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MixinRegistration[] GetRepositoryMixinsRegistrations()
        {
            return new[] {
                new MixinRegistration(typeof(IR3A.IUserAccountEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3A.IPasswordEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3A.IUserRoleEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3A.IUserRoleEntity), typeof(IR3A.IEntityPartUserRole<IR3B.MyAppUserRole>))
            };
        }
    }
}
