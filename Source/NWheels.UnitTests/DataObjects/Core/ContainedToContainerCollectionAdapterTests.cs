using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.DataObjects.Core;
using NWheels.TypeModel.Core;
using Shouldly;

namespace NWheels.UnitTests.DataObjects.Core
{
    [TestFixture]
    public class ContainedToContainerCollectionAdapterTests
    {
        private int _outerFactoryInvocationCount;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _outerFactoryInvocationCount = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateOuterItemsUponEnumeration()
        {
            //-- arrange

            var inner1 = new InnerImpl();
            var inner2 = new InnerImpl();
            var inner3 = new InnerImpl();
            
            var innerCollection = new List<IConcreteInnerContract> { inner1, inner2, inner3 };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            //-- act

            var outerItems = outerCollection.ToArray();

            //-- assert

            outerCollection.IsChanged.ShouldBe(false);
            _outerFactoryInvocationCount.ShouldBe(3);
            
            outerItems.Length.ShouldBe(3);
            outerItems.ShouldAllBe(outer => outer is OuterImpl);
            outerItems.Cast<OuterImpl>().Select(outer => outer.Inner).ShouldBe(new[] { inner1, inner2, inner3 });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void OuterItemsAreCreatedOnlyOnce()
        {
            //-- arrange

            var inner1 = new InnerImpl();
            var inner2 = new InnerImpl();
            var inner3 = new InnerImpl();

            var innerCollection = new List<IConcreteInnerContract> { inner1, inner2, inner3 };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            //-- act

            var outerItems1 = outerCollection.ToArray();
            var outerItems2 = outerCollection.ToArray();

            //-- assert

            _outerFactoryInvocationCount.ShouldBe(3);
            outerItems2.ShouldBe(outerItems1);

            outerCollection.IsChanged.ShouldBe(false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanTrackChangedItems()
        {
            //-- arrange

            var innerCollection = new List<IConcreteInnerContract> { new InnerImpl(), new InnerImpl(), new InnerImpl() };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            IConcreteOuterContract[] added;
            IConcreteOuterContract[] changed;
            IConcreteOuterContract[] removed;

            //-- act

            ((OuterImpl)outerCollection.First()).IsModified = true;
            ((OuterImpl)outerCollection.Last()).IsModified = true;

            outerCollection.GetChanges(out added, out changed, out removed);

            //-- assert

            outerCollection.IsChanged.ShouldBe(true);

            added.ShouldBe(null);
            removed.ShouldBe(null);
            changed.ShouldBe(new[] { outerCollection.First(), outerCollection.Last() });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanTrackAddedItems()
        {
            //-- arrange

            var innerCollection = new List<IConcreteInnerContract> { new InnerImpl(), new InnerImpl() };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            IConcreteOuterContract[] added;
            IConcreteOuterContract[] changed;
            IConcreteOuterContract[] removed;

            var newOuterItems = new[] { new OuterImpl(), new OuterImpl() };
            var newInnerItems = new[] { new InnerImpl(), new InnerImpl() };
            newOuterItems[0].Inner = newInnerItems[0];
            newInnerItems[0].Outer = newOuterItems[0];
            newOuterItems[1].Inner = newInnerItems[1];
            newInnerItems[1].Outer = newOuterItems[1];

            //-- act

            outerCollection.Add(newOuterItems[0]);
            outerCollection.Add(newOuterItems[1]);
            outerCollection.GetChanges(out added, out changed, out removed);

            //-- assert

            outerCollection.IsChanged.ShouldBe(true);

            removed.ShouldBe(null);
            added.ShouldBe(new IConcreteOuterContract[] { newOuterItems[0], newOuterItems[1] });
            changed.ShouldBe(null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanTrackRemovedItems()
        {
            //-- arrange

            var inner1 = new InnerImpl();
            var inner2 = new InnerImpl();
            var inner3 = new InnerImpl();
            var inner4 = new InnerImpl();
            
            var innerCollection = new List<IConcreteInnerContract> { inner1, inner2, inner3, inner4 };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            IConcreteOuterContract[] added;
            IConcreteOuterContract[] changed;
            IConcreteOuterContract[] removed;

            //-- act

            outerCollection.Remove(outerCollection.First());
            outerCollection.Remove(outerCollection.Last());
            outerCollection.GetChanges(out added, out changed, out removed);

            //-- assert

            outerCollection.IsChanged.ShouldBe(true);

            changed.ShouldBe(null);
            added.ShouldBe(null);
            removed.ShouldBe(new[] { (IConcreteOuterContract)inner1.Outer, (IConcreteOuterContract)inner4.Outer });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanTrackRemovedItemsByClear()
        {
            //-- arrange

            var inner1 = new InnerImpl();
            var inner2 = new InnerImpl();
            var inner3 = new InnerImpl();

            var innerCollection = new List<IConcreteInnerContract> { inner1, inner2, inner3 };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            IConcreteOuterContract[] added;
            IConcreteOuterContract[] changed;
            IConcreteOuterContract[] removed;

            //-- act

            outerCollection.Clear();
            outerCollection.GetChanges(out added, out changed, out removed);

            //-- assert

            outerCollection.IsChanged.ShouldBe(true);

            changed.ShouldBe(null);
            added.ShouldBe(null);
            removed.ShouldAllBe(r => r != null);
            removed.ShouldBe(new[] { (IConcreteOuterContract)inner1.Outer, (IConcreteOuterContract)inner2.Outer, (IConcreteOuterContract)inner3.Outer });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AddedItemsAreNeverReportedAsRemoved()
        {
            //-- arrange

            var inner1 = new InnerImpl();
            var inner2 = new InnerImpl();
            var inner3 = new InnerImpl();

            var innerCollection = new List<IConcreteInnerContract> { inner1, inner2, inner3 };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            IConcreteOuterContract[] added;
            IConcreteOuterContract[] changed;
            IConcreteOuterContract[] removed;

            var newOuterItems = new[] { new OuterImpl(), new OuterImpl() };
            var newInnerItems = new[] { new InnerImpl(), new InnerImpl() };
            newOuterItems[0].Inner = newInnerItems[0];
            newInnerItems[0].Outer = newOuterItems[0];
            newOuterItems[1].Inner = newInnerItems[1];
            newInnerItems[1].Outer = newOuterItems[1];

            //-- act

            outerCollection.Add(newOuterItems[0]);
            outerCollection.Add(newOuterItems[1]);
            outerCollection.Remove(newOuterItems[1]);
            outerCollection.Clear();
            outerCollection.GetChanges(out added, out changed, out removed);

            //-- assert

            outerCollection.IsChanged.ShouldBe(true);

            changed.ShouldBe(null);
            added.ShouldBe(null);
            removed.ShouldAllBe(r => r != null);
            removed.ShouldBe(new[] { (IConcreteOuterContract)inner1.Outer, (IConcreteOuterContract)inner2.Outer, (IConcreteOuterContract)inner3.Outer });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RemovedItemsAreNeverReportedAsChanged()
        {
            //-- arrange
            
            var inner1 = new InnerImpl();
            var inner2 = new InnerImpl();
            var inner3 = new InnerImpl();

            var innerCollection = new List<IConcreteInnerContract> { inner1, inner2, inner3 };
            var outerCollection =
                new ContainedToContainerCollectionAdapter<IAbstractInner, IAbstractOuter, IConcreteInnerContract, IConcreteOuterContract>(
                    innerCollection,
                    OuterFactory);

            IConcreteOuterContract[] added;
            IConcreteOuterContract[] changed;
            IConcreteOuterContract[] removed;

            //-- act

            ((OuterImpl)outerCollection.First()).IsModified = true;
            ((OuterImpl)outerCollection.Last()).IsModified = true;
            outerCollection.Clear();

            outerCollection.GetChanges(out added, out changed, out removed);

            //-- assert

            outerCollection.IsChanged.ShouldBe(true);

            added.ShouldBe(null);
            changed.ShouldBe(null);
            removed.ShouldBe(new[] { (IConcreteOuterContract)inner1.Outer, (IConcreteOuterContract)inner2.Outer, (IConcreteOuterContract)inner3.Outer });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IAbstractOuter OuterFactory(IAbstractInner inner)
        {
            _outerFactoryInvocationCount++;

            var concreteInner = (InnerImpl)inner;
            var concreteOuter = new OuterImpl();
            concreteInner.Outer = concreteOuter;
            concreteOuter.Inner = concreteInner;

            return concreteInner.Outer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAbstractOuter : IContain<IAbstractInner> { }
        public interface IAbstractInner : IContainedIn<IAbstractOuter> { }
        public interface IConcreteOuterContract { }
        public interface IConcreteInnerContract { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OuterImpl : IAbstractOuter, IConcreteOuterContract, IObject
        {
            public IAbstractInner GetContainedObject()
            {
                return Inner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAbstractInner Inner { get; set; }
            public Type ContractType { get; set; }
            public Type FactoryType { get; set; }
            public bool IsModified { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InnerImpl : IAbstractInner, IConcreteInnerContract
        {
            public IAbstractOuter GetContainerObject()
            {
                return Outer;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IAbstractOuter Outer { get; set; }
        }
    }
}
