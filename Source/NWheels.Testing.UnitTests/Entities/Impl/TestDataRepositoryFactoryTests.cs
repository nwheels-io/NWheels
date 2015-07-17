using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;
using NWheels.Testing.Entities.Impl;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Testing.UnitTests.Entities.Impl
{
    [TestFixture]
    public class TestDataRepositoryFactoryTests : DynamicTypeUnitTestBase
    {
        private TestDataRepositoryFactory _factoryUnderTest;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _factoryUnderTest = new TestDataRepositoryFactory(
                base.DyamicModule,
                (TypeMetadataCache)Framework.MetadataCache,
                new EntityObjectFactory(Framework.Components, base.DyamicModule, (TypeMetadataCache)Framework.MetadataCache));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateTestDataRepository()
        {
            //-- act

            var repo = _factoryUnderTest.CreateService<IR1.IOnlineStoreRepository>();

            //-- assert

            Assert.That(repo, Is.Not.Null);
            Assert.That(repo.GetEntityTypesInRepository().Length, Is.EqualTo(3));

            repo.GetEntityTypesInRepository().Single(t => typeof(IR1.IOrder).IsAssignableFrom(t));
            repo.GetEntityTypesInRepository().Single(t => typeof(IR1.IProduct).IsAssignableFrom(t));
            repo.GetEntityTypesInRepository().Single(t => typeof(IR1.IOrderLine).IsAssignableFrom(t));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformBasicCrudOperations()
        {
            //-- arrange

            var repo = _factoryUnderTest.CreateService<IR1.IOnlineStoreRepository>();

            //-- act & assert

            CrudOperations.Repository1.ExecuteBasic(repoFactory: () => TestDataRepositoryBase.ResetState(repo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformAdvancedRetrievals()
        {
            //-- arrange

            var repo = _factoryUnderTest.CreateService<IR1.IOnlineStoreRepository>();

            //-- act & assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(repoFactory: () => TestDataRepositoryBase.ResetState(repo));
        }
    }
}
