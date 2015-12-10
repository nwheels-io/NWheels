using System;
using System.Collections.Generic;
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
using NWheels.Testing.Entities.Impl;
using NWheels.Testing.Entities.Stacks;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;
using NWheels.Entities.Core;
using NWheels.Entities;

namespace NWheels.Testing.UnitTests.Entities.Impl
{
    [TestFixture]
    public class TestDataRepositoryFactoryTests : DynamicTypeUnitTestBase
    {
        private TestDataRepositoryFactory _factoryUnderTest;
        private TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            Framework.RebuildMetadataCache(
                customMetadataConventions: new IMetadataConvention[] {
                    new DefaultIdMetadataConvention(typeof(int))
                }
            );

            _metadataCache = (TypeMetadataCache)Framework.MetadataCache;

            var attributeMetaType = _metadataCache.GetTypeMetadata(typeof(IR1.IAttribute));
            var categoryMetaType = _metadataCache.GetTypeMetadata(typeof(IR1.ICategory));
            var productMetaType = _metadataCache.GetTypeMetadata(typeof(IR1.IProduct));
            var orderMetaType = _metadataCache.GetTypeMetadata(typeof(IR1.IOrder));
            var orderLineMetaType = _metadataCache.GetTypeMetadata(typeof(IR1.IOrderLine));

            _metadataCache.AcceptVisitor(new CrossTypeFixupMetadataVisitor(_metadataCache));

            var entityObjectFactory = new TestEntityObjectFactory(Framework.Components, base.DyamicModule, _metadataCache);

            _factoryUnderTest = new TestDataRepositoryFactory(
                Framework.Components,
                base.DyamicModule,
                _metadataCache,
                new IDatabaseNameResolver[0],
                entityObjectFactory);

            Framework.UpdateComponents(
                builder => {
                    builder.RegisterInstance(entityObjectFactory).As<IEntityObjectFactory, EntityObjectFactory, TestEntityObjectFactory>();
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateEntityObjects()
        {
            //-- arrange

            var entityFactory = new TestEntityObjectFactory(Framework.Components, base.DyamicModule, _metadataCache);

            //-- act

            var attributeEntity = entityFactory.NewEntity<IR1.IAttribute>();
            var categoryEntity = entityFactory.NewEntity<IR1.ICategory>();
            var productEntity = entityFactory.NewEntity<IR1.IProduct>();
            var orderEntity = entityFactory.NewEntity<IR1.IOrder>();
            var orderLineEntity = entityFactory.NewEntity<IR1.IOrderLine>();

            //-- assert

            Assert.That(attributeEntity, Is.InstanceOf<IR1.IAttribute>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
            Assert.That(categoryEntity, Is.InstanceOf<IR1.ICategory>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
            Assert.That(productEntity, Is.InstanceOf<IR1.IProduct>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
            Assert.That(orderEntity, Is.InstanceOf<IR1.IOrder>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
            Assert.That(orderLineEntity, Is.InstanceOf<IR1.IOrderLine>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateTestDataRepository()
        {
            //-- act

            var repo = _factoryUnderTest.CreateService<IR1.IOnlineStoreRepository>();

            //-- assert

            Assert.That(repo, Is.Not.Null);
            Assert.That(repo.GetEntityTypesInRepository().Length, Is.EqualTo(15));

            repo.GetEntityTypesInRepository().Single(t => typeof(IR1.ICategory).IsAssignableFrom(t));
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

            CrudOperations.Repository1.ExecuteBasic(Framework, repoFactory: () => TestDataRepositoryBase.ResetState(repo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPerformAdvancedRetrievals()
        {
            //-- arrange

            var repo = _factoryUnderTest.CreateService<IR1.IOnlineStoreRepository>();

            //-- act & assert

            CrudOperations.Repository1.ExecuteAdvancedRetrievals(Framework, repoFactory: () => TestDataRepositoryBase.ResetState(repo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DynamicTypeUnitTestBase

        protected override DynamicModule CreateDynamicModule()
        {
            return base.DyamicModule;
        }

        #endregion
    }
}
