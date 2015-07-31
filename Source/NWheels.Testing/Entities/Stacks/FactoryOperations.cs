using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Testing.Entities.Impl;
using IR1 = NWheels.Testing.Entities.Stacks.Interfaces.Repository1;

namespace NWheels.Testing.Entities.Stacks
{
    public static class FactoryOperations
    {
        public static class Repository1
        {
            public static void ExecuteEntityCreation(TestFramework framework, Func<EntityObjectFactory> factoryFactory)
            {
                //-- arrange

                SetupFrameworkForEntityFactory(framework);
                var factory = factoryFactory();

                //-- act

                var attributeEntity = factory.NewEntity<IR1.IAttribute>();
                var categoryEntity = factory.NewEntity<IR1.ICategory>();
                var productEntity = factory.NewEntity<IR1.IProduct>();
                var orderEntity = factory.NewEntity<IR1.IOrder>();
                var orderLineEntity = factory.NewEntity<IR1.IOrderLine>();

                //-- assert

                Assert.That(attributeEntity, Is.InstanceOf<IR1.IAttribute>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
                Assert.That(categoryEntity, Is.InstanceOf<IR1.ICategory>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
                Assert.That(productEntity, Is.InstanceOf<IR1.IProduct>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
                Assert.That(orderEntity, Is.InstanceOf<IR1.IOrder>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
                Assert.That(orderLineEntity, Is.InstanceOf<IR1.IOrderLine>().And.InstanceOf<IEntityObject>().And.InstanceOf<IEntityPartId<int>>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void SetupFrameworkForEntityFactory(TestFramework framework)
            {
                var metadataCache = TestFramework.CreateMetadataCacheWithDefaultConventions(new IMetadataConvention[] {
                    new DefaultIdMetadataConvention(typeof(int)),
                    new TestIdMetadataConvention(), 
                });

                var updater = new ContainerBuilder();
                updater.RegisterInstance(metadataCache).As<ITypeMetadataCache, TypeMetadataCache>();
                updater.Update(framework.Components.ComponentRegistry);

                var attributeMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IAttribute));
                var categoryMetaType = metadataCache.GetTypeMetadata(typeof(IR1.ICategory));
                var productMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IProduct));
                var orderMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IOrder));
                var orderLineMetaType = metadataCache.GetTypeMetadata(typeof(IR1.IOrderLine));

                metadataCache.AcceptVisitor(new CrossTypeFixupMetadataVisitor(metadataCache));
            }
        }
    }
}
