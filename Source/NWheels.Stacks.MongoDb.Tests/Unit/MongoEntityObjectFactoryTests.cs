using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using MongoDB.Bson;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.Testing;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Stacks.MongoDb.Tests.Unit
{
    [TestFixture]
    public class MongoEntityObjectFactoryTests : DynamicTypeUnitTestBase
    {
        [Test]
        public void CanCreateEntityObjects()
        {
            //-- arrange

            var factory = CreateEntityObjectFactory();

            //-- act

            var attributeEntity = factory.NewEntity<IR1.IAttribute>();
            var categoryEntity = factory.NewEntity<IR1.ICategory>();
            var productEntity = factory.NewEntity<IR1.IProduct>();
            var orderEntity = factory.NewEntity<IR1.IOrder>();
            var orderLineEntity = factory.NewEntity<IR1.IOrderLine>();

            //-- assert

            Assert.That(attributeEntity, Is.InstanceOf<IR1.IAttribute>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<ObjectId>>());
            Assert.That(categoryEntity, Is.InstanceOf<IR1.ICategory>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<ObjectId>>());
            Assert.That(productEntity, Is.InstanceOf<IR1.IProduct>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<ObjectId>>());
            Assert.That(orderEntity, Is.InstanceOf<IR1.IOrder>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<ObjectId>>());
            Assert.That(orderLineEntity, Is.InstanceOf<IR1.IOrderLine>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<ObjectId>>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MongoEntityObjectFactory CreateEntityObjectFactory()
        {
            var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(
                Framework.Components,
                new IMetadataConvention[] {
                    new DefaultIdMetadataConvention(typeof(ObjectId))
                });
            
            var updater = new ContainerBuilder();
            updater.RegisterInstance(metadataCache).As<ITypeMetadataCache, TypeMetadataCache>();
            updater.Update(Framework.Components.ComponentRegistry);

            var attributeMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IAttribute));
            var categoryMetaType = metadataCache.GetTypeMetadata(typeof(IR1.ICategory));
            var productMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IProduct));
            var orderMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IOrder));
            var orderLineMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IOrderLine));

            metadataCache.AcceptVisitor(new CrossTypeFixupMetadataVisitor(metadataCache));

            var factory = new MongoEntityObjectFactory(base.Framework.Components, base.DyamicModule, metadataCache);

            Framework.UpdateComponents(
                builder => {
                    builder.RegisterInstance(factory).As<IEntityObjectFactory, EntityObjectFactory, MongoEntityObjectFactory>();
                });

            return factory;
        }
    }
}
