using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Testing.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Core.UnitTests.DataObjects
{
    [TestFixture]
    public class TypeMetadataCacheTests
    {
        [Test]
        public void CanBuildScalarProperties()
        {
            //-- Arrange

            var cache = new TypeMetadataCache(CreateMetadataConventionSet());

            //-- Act

            var product = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IProduct));

            //-- Assert

            Assert.That(
                product.Properties.Select(p => p.Name).ToArray(), 
                Is.EqualTo(new[] { "Id", "Name", "Price", "Description" }));
            
            Assert.That(
                product.Properties.Select(p => p.ClrType.Name).ToArray(), 
                Is.EqualTo(new[] { "Int32", "String", "Decimal", "String" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanIncludePropertyDefaultValue()
        {
            //-- Arrange

            var cache = new TypeMetadataCache(CreateMetadataConventionSet());

            //-- Act

            var orderType = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrder));
            var statusProperty = orderType.GetPropertyByName("Status");

            //-- Assert

            Assert.That(statusProperty.DefaultValue, Is.EqualTo(TestDataObjects.Repository1.OrderStatus.New));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuildScalarPrimaryKey()
        {
            //-- Arrange

            var cache = new TypeMetadataCache(CreateMetadataConventionSet());

            //-- Act

            var product = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IProduct));

            //-- Assert

            Assert.That(product.PrimaryKey.Properties.Single().Name, Is.EqualTo("Id"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuildRelationsBetweenMutuallyDependentTypes()
        {
            //-- Arrange

            var cache = new TypeMetadataCache(CreateMetadataConventionSet());

            //-- Act

            var orderMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrder));
            var orderLineMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrderLine));

            //-- Assert

            var orderToOrderLine = orderMetadata.Properties.Single(p => p.Name == "OrderLines").Relation;
            var orderLineToOrder = orderLineMetadata.Properties.Single(p => p.Name == "Order").Relation;
            var orderLineToProduct = orderLineMetadata.Properties.Single(p => p.Name == "Product").Relation;

            Assert.That(Jsonlike.Stringify(orderToOrderLine), Is.EqualTo(
                "{relationKind:OneToMany,thisPartyKind:Principal,thisPartyKey:PK_Order,relatedPartyType:OrderLine,relatedPartyKind:Dependent,relatedPartyKey:FK_Order}"
            ));

            Assert.That(Jsonlike.Stringify(orderLineToOrder), Is.EqualTo(
                "{relationKind:ManyToOne,thisPartyKind:Dependent,thisPartyKey:FK_Order,relatedPartyType:Order,relatedPartyKind:Principal,relatedPartyKey:PK_Order}"
            ));

            Assert.That(Jsonlike.Stringify(orderLineToProduct), Is.EqualTo(
                "{relationKind:ManyToOne,thisPartyKind:Dependent,thisPartyKey:FK_Product,relatedPartyType:Product,relatedPartyKind:Principal,relatedPartyKey:PK_Product}"
            ));

            //Console.WriteLine(JsonlikeMetadataStringifier.Stringify(orderMetadata));
            //Console.WriteLine(JsonlikeMetadataStringifier.Stringify(productMetadata));
            //Console.WriteLine(JsonlikeMetadataStringifier.Stringify(orderLineMetadata));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanIncludeMixinContracts()
        {
            //-- Arrange

            var cache = new TypeMetadataCache(
                CreateMetadataConventionSet(), 
                new[] {
                    new MixinRegistration(typeof(TestDataObjects.Repository2.IPrimaryContract), typeof(TestDataObjects.Repository2.IFirstMixinContract)),
                    new MixinRegistration(typeof(TestDataObjects.Repository2.IPrimaryContract), typeof(TestDataObjects.Repository2.ISecondMixinContract))
                },
                new ConcretizationRegistration[0]);

            //-- Act

            var primaryContractMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository2.IPrimaryContract));

            //-- Assert

            Assert.That(primaryContractMetadata.ContractType, Is.EqualTo(typeof(TestDataObjects.Repository2.IPrimaryContract)));
            Assert.That(primaryContractMetadata.MixinContractTypes, Is.EquivalentTo(new[] {
                typeof(TestDataObjects.Repository2.IFirstMixinContract),
                typeof(TestDataObjects.Repository2.ISecondMixinContract)
            }));

            var propertyNames = primaryContractMetadata.Properties.Select(p => p.Name).ToArray();

            Assert.That(propertyNames, Is.EquivalentTo(new[] {
                "PrimaryProperty", "FirstMixinProperty", "SecondMixinPropertyA", "SecondMixinPropertyB"
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MetadataConventionSet CreateMetadataConventionSet()
        {
            return new MetadataConventionSet(
                new IMetadataConvention[] { new ContractMetadataConvention(), new AttributeMetadataConvention(), new RelationMetadataConvention()  }, 
                new IRelationalMappingConvention[] { new PascalCaseRelationalMappingConvention(usePluralTableNames: true)  });
        }
    }
}
