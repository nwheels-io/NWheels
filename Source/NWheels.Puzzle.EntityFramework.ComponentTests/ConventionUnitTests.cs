using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Puzzle.EntityFramework.Conventions;

namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    [TestFixture]
    public class ConventionUnitTests : NUnitEmittedTypesTestBase
    {
        [Test]
        public void CanCreateFlatEntityObject()
        {
            //-- Arrange

            var factory = new EntityObjectFactory(base.Module);

            //-- Act

            var obj = factory.NewEntity<Interfaces.Repository1.IProduct>();

            obj.Id = 123;
            obj.Name = "ABC";
            obj.Price = 123.45m;

            //-- Assert

            Assert.That(obj.Id,Is.EqualTo(123));
            Assert.That(obj.Name, Is.EqualTo("ABC"));
            Assert.That(obj.Price, Is.EqualTo(123.45m));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateEntityObjectsWithNavigations()
        {
            //-- Arrange

            var factory = new EntityObjectFactory(base.Module);

            //-- Act

            var product = factory.NewEntity<Interfaces.Repository1.IProduct>();
            var order = factory.NewEntity<Interfaces.Repository1.IOrder>();
            var orderLine = factory.NewEntity<Interfaces.Repository1.IOrderLine>();

            orderLine.Order = order;
            orderLine.Product = product;
            order.OrderLines.Add(orderLine);

            //-- Assert

            Assert.That(orderLine.Order, Is.SameAs(order));
            Assert.That(orderLine.Product, Is.SameAs(product));
            Assert.That(order.OrderLines.Count, Is.EqualTo(1));
            Assert.That(order.OrderLines.First(), Is.SameAs(orderLine));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Test]
        //public void CanCreateRepositoryContextObject()
        //{
        //    //-- Arrange

        //    var factory = new EntityRepositoryFactory(base.Module);

        //    //-- Act

        //    var product = factory.NewEntity<Interfaces.Repository1.IProduct>();
        //    var order = factory.NewEntity<Interfaces.Repository1.IOrder>();
        //    var orderLine = factory.NewEntity<Interfaces.Repository1.IOrderLine>();

        //    orderLine.Order = order;
        //    orderLine.Product = product;
        //    order.OrderLines.Add(orderLine);

        //    //-- Assert

        //    Assert.That(orderLine.Order, Is.SameAs(order));
        //    Assert.That(orderLine.Product, Is.SameAs(product));
        //    Assert.That(order.OrderLines.Count, Is.EqualTo(1));
        //    Assert.That(order.OrderLines.First(), Is.SameAs(orderLine));
        //}
    }
}
