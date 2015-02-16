using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Core.DataObjects;
using NWheels.DataObjects;

namespace NWheels.Core.UnitTests.DataObjects
{
    [TestFixture]
    public class TypeMetadataCacheTests
    {
        [Test]
        public void CanBuildScalarProperties()
        {
            //-- Arrange

            var cache = new TypeMetadataCache();

            //-- Act

            var product = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IProduct));

            //-- Assert

            Assert.That(
                product.Properties.Select(p => p.Name).ToArray(), 
                Is.EqualTo(new[] { "Id", "Name", "Price" }));
            
            Assert.That(
                product.Properties.Select(p => p.ClrType.Name).ToArray(), 
                Is.EqualTo(new[] { "Int32", "String", "Decimal" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuildScalarPrimaryKey()
        {
            //-- Arrange

            var cache = new TypeMetadataCache();

            //-- Act

            var product = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IProduct));

            //-- Assert

            Assert.That(product.PrimaryKey.Properties.Single().Name, Is.EqualTo("Id"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBuiltRelationsBetweenMutuallyDependentTypes()
        {
            //-- Arrange

            var cache = new TypeMetadataCache();

            //-- Act

            var orderMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrder));
            var orderLineMetadata = cache.GetTypeMetadata(typeof(TestDataObjects.Repository1.IOrderLine));

            //-- Assert

            var orderToOrderLine = orderMetadata.Properties.Single(p => p.Name == "OrderLines").Relation;
            var orderLineToOrder = orderLineMetadata.Properties.Single(p => p.Name == "Order").Relation;
            var orderLineToProdyct = orderLineMetadata.Properties.Single(p => p.Name == "Product").Relation;

            Assert.That(Jsonlike.Stringify(orderToOrderLine), Is.EqualTo(
                "{relationKind:OneToMany,thisPartyKind:Principal,thisPartyKey:PK_Order,relatedPartyType:OrderLine,relatedPartyKind:Dependent,relatedPartyKey:FK_Order}"
            ));

            Assert.That(Jsonlike.Stringify(orderLineToOrder), Is.EqualTo(
                "{relationKind:ManyToOne,thisPartyKind:Dependent,thisPartyKey:FK_Order,relatedPartyType:Order,relatedPartyKind:Principal,relatedPartyKey:PK_Order}"
            ));

            Assert.That(Jsonlike.Stringify(orderLineToProdyct), Is.EqualTo(
                "{relationKind:ManyToOne,thisPartyKind:Dependent,thisPartyKey:FK_Product,relatedPartyType:Product,relatedPartyKind:Principal,relatedPartyKey:PK_Product}"
            ));

            //Console.WriteLine(JsonlikeMetadataStringifier.Stringify(orderMetadata));
            //Console.WriteLine(JsonlikeMetadataStringifier.Stringify(productMetadata));
            //Console.WriteLine(JsonlikeMetadataStringifier.Stringify(orderLineMetadata));
        }
    }
}
