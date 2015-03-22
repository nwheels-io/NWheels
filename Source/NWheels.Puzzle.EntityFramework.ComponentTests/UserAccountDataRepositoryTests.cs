using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.Core.Entities;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Puzzle.EntityFramework.Conventions;
using NWheels.Puzzle.EntityFramework.Impl;
using NWheels.Testing.DataObjects;
using IR3 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository3;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
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

            var metadataCache = new TypeMetadataCache(new DataObjectConventions(), new PascalCaseRelationalMappingConvention(usePluralTableNames: true));

            //-- Act

            var userAccountMetadata = metadataCache.GetTypeMetadata(typeof(IR3.IUserAccountEntity));

            //-- Assert

            Console.WriteLine(Jsonlike.Stringify(userAccountMetadata));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateEntityObjects()
        {
            //-- Arrange

            var metadataCache = new TypeMetadataCache(new DataObjectConventions(), new PascalCaseRelationalMappingConvention(usePluralTableNames: true));
            var entityObjectFactory = new EfEntityObjectFactory(_dynamicModule, metadataCache);

            //-- Act

            var password = entityObjectFactory.NewEntity<IR3.IPasswordEntity>();
            var role = entityObjectFactory.NewEntity<IR3.IUserRoleEntity>();
            var userAccount = entityObjectFactory.NewEntity<IR3.IUserAccountEntity>();

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
                repoFactory.CreateDataRepository<IR3.IMyDataRepository>(connection, autoCommit: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EfDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var mixinRegistrations = GetRepositoryMixinsRegistrations();

            var metadataCache = new TypeMetadataCache(
                new DataObjectConventions(), 
                new PascalCaseRelationalMappingConvention(usePluralTableNames: true), 
                mixinRegistrations);

            var entityFactory = new EfEntityObjectFactory(_dynamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(_dynamicModule, entityFactory, metadataCache);

            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IR3.IMyDataRepository InitializeDataRepository()
        {
            var repoFactory = CreateDataRepositoryFactory();

            var connection = base.CreateDbConnection();
            var repo = repoFactory.CreateDataRepository<IR3.IMyDataRepository>(connection, autoCommit: true);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            return (IR3.IMyDataRepository)repo;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MixinRegistration[] GetRepositoryMixinsRegistrations()
        {
            return new[] {
                new MixinRegistration(typeof(IR3.IUserAccountEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3.IPasswordEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3.IUserRoleEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3.IUserAccountEntity), typeof(IR3.IEntityPartUserRoleId<IR3.MyUserRole>))
            };
        }
    }
}
