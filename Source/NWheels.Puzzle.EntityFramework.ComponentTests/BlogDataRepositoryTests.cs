using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.Core.Entities;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Puzzle.EntityFramework.Conventions;
using NWheels.Puzzle.EntityFramework.Impl;
using IR2 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository2;
using IR3 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository3A;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture, Category("Integration")]
    public class BlogDataRepositoryTests : DatabaseTestBase
    {
        private Hapil.DynamicModule _dynamicModule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _dynamicModule = new DynamicModule(
                "EmittedByBlogDataRepositoryTests",
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
        public void CanCreateBlogEntitiesMetadata()
        {
            //-- Act

            var metadataCache = base.CreateMetadataCache(GetRepositoryMixinsRegistrations());

            //-- Assert

            //TODO
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateBlogEntities()
        {
            //-- Arrange

            var metadataCache = base.CreateMetadataCache(GetRepositoryMixinsRegistrations());
            var entityFactory = new EfEntityObjectFactory(_dynamicModule, metadataCache);

            //-- Act

            var author = entityFactory.NewEntity<IR2.IAuthorEntity>();
            var article = entityFactory.NewEntity<IR2.IArticleEntity>();
            var post = entityFactory.NewEntity<IR2.IPostEntity>();
            var tag = entityFactory.NewEntity<IR2.ITagEntity>();

            //-- Assert

            //TODO
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, Ignore("WIP")]
        public void CanCreateDataRepositoryObject()
        {
            var repoFactory = CreateDataRepositoryFactory();

            using ( var connection = base.CreateDbConnection() )
            {
                repoFactory.CreateDataRepository<IR2.IBlogDataRepository>(connection, autoCommit: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EfDataRepositoryFactory CreateDataRepositoryFactory()
        {
            var metadataCache = base.CreateMetadataCache(GetRepositoryMixinsRegistrations());
            var entityFactory = new EfEntityObjectFactory(_dynamicModule, metadataCache);
            var repoFactory = new EfDataRepositoryFactory(_dynamicModule, entityFactory, metadataCache, SqlClientFactory.Instance, ResolveAuto<IDatabaseConfig>());
            return repoFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IR2.IBlogDataRepository InitializeDataRepository()
        {
            var repoFactory = CreateDataRepositoryFactory();

            var connection = base.CreateDbConnection();
            var repo = repoFactory.CreateDataRepository<IR2.IBlogDataRepository>(connection, autoCommit: true);

            base.CompiledModel = ((EfDataRepositoryBase)repo).CompiledModel;

            return (IR2.IBlogDataRepository)repo;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MixinRegistration[] GetRepositoryMixinsRegistrations()
        {
            return new[] {
                new MixinRegistration(typeof(IR3.IUserAccountEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3.IPasswordEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3.IUserRoleEntity), typeof(IEntityPartId<int>)),
                new MixinRegistration(typeof(IR3.IUserRoleEntity), typeof(IR3.IEntityPartUserRole<IR2.UserRole>))
            };
        }
    }
}
