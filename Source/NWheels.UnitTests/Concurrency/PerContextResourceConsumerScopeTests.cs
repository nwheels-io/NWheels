using System;
using NUnit.Framework;
using NWheels.Concurrency;
using Shouldly;

namespace NWheels.UnitTests.Concurrency
{
    [TestFixture]
    public class PerContextResourceConsumerScopeTests
    {
        [SetUp]
        public void Setup()
        {
            TestScope.LastScopeId = 0;
            TestResource.LastResourceId = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Outermost(
            [Values(true, false)] bool externallyOwned,
            [Values(true, false)] bool forceNewResource,
            [Values(true, false)] bool resourceIsScopeHandler)
        {
            //-- arrange

            var factoryCount = 0;
            var anchor = new TestAnchor();

            TestResource resourceCreatedByFactory = null;
            TestScope scopeUnderTest;
            PerContextResourceConsumerScope<TestResource> observedCurrentScope;
            TestResource observedCurrentResource;

            //-- act

            using (scopeUnderTest = new TestScope(
                anchor,
                resourceFactory: h => {
                    factoryCount++;
                    resourceCreatedByFactory = resourceIsScopeHandler ? new ScopedTestResource() : new TestResource();
                    return resourceCreatedByFactory;
                },
                externallyOwned: externallyOwned,
                forceNewResource: forceNewResource))
            {
                observedCurrentScope = anchor.Current;
                observedCurrentResource = anchor.Current.Resource;
            }

            //-- assert

            anchor.Current.ShouldBe(null);
            
            observedCurrentScope.ShouldBeSameAs(scopeUnderTest);
            observedCurrentResource.ShouldBeSameAs(resourceCreatedByFactory);
            
            factoryCount.ShouldBe(1);
            resourceCreatedByFactory.DisposeCount.ShouldBe(externallyOwned? 0 : 1);

            CheckScopeChangedCount(resourceIsScopeHandler, resourceCreatedByFactory, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NestedOneLevelUsingSameResource(
            [Values(true, false)] bool externallyOwned,
            [Values(true, false)] bool resourceIsScopeHandler)
        {
            //-- arrange

            var factoryCount = 0;
            var anchor = new TestAnchor();

            TestResource resourceCreatedByFactory = null;

            Func<IResourceConsumerScopeHandle, TestResource> resourceFactory = h => {
                var resource = resourceIsScopeHandler ? new ScopedTestResource() : new TestResource();
                resourceCreatedByFactory = resource;
                factoryCount++;
                return resource;
            };

            PerContextResourceConsumerScope<TestResource> observedCurrentScope;
            TestResource observedCurrentResource;
            int observedDisposeCount;

            PerContextResourceConsumerScope<TestResource> observedPostCurrentScope;
            TestResource observedPostCurrentResource;

            TestScope outermostScope;
            TestScope scopeUnderTest;

            //-- act

            using (outermostScope = new TestScope(anchor, resourceFactory, externallyOwned: externallyOwned, forceNewResource: false))
            {
                using (scopeUnderTest = new TestScope(anchor, resourceFactory, externallyOwned: externallyOwned, forceNewResource: false))
                {
                    observedCurrentScope = anchor.Current;
                    observedCurrentResource = anchor.Current.Resource;
                }

                observedDisposeCount = resourceCreatedByFactory.DisposeCount;
                observedPostCurrentScope = anchor.Current;
                observedPostCurrentResource = anchor.Current.Resource;
            }

            //-- assert

            anchor.Current.ShouldBe(null);

            observedCurrentScope.ShouldBeSameAs(scopeUnderTest);
            observedCurrentResource.ShouldBeSameAs(resourceCreatedByFactory);

            observedPostCurrentScope.ShouldBeSameAs(outermostScope);
            observedPostCurrentResource.ShouldBeSameAs(resourceCreatedByFactory);

            factoryCount.ShouldBe(1);
            observedDisposeCount.ShouldBe(0);
            resourceCreatedByFactory.DisposeCount.ShouldBe(externallyOwned ? 0 : 1);

            CheckScopeChangedCount(resourceIsScopeHandler, resourceCreatedByFactory, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NestedOneLevelForcingNewResource(
            [Values(true, false)] bool externallyOwned,
            [Values(true, false)] bool resourceIsScopeHandler)
        {
            //-- arrange

            var factoryCount = 0;
            var anchor = new TestAnchor();

            TestResource[] resourceCreatedByFactory = new TestResource[2];

            Func<IResourceConsumerScopeHandle, TestResource> resourceFactory = h =>
            {
                var resource = resourceIsScopeHandler ? new ScopedTestResource() : new TestResource();
                resourceCreatedByFactory[factoryCount] = resource;
                factoryCount++;
                return resource;
            };

            PerContextResourceConsumerScope<TestResource> observedCurrentScope;
            TestResource observedCurrentResource;
            int observedDisposeCount;

            PerContextResourceConsumerScope<TestResource> observedPostCurrentScope;
            TestResource observedPostCurrentResource;

            TestScope outermostScope;
            TestScope scopeUnderTest;

            //-- act

            using (outermostScope = new TestScope(anchor, resourceFactory, externallyOwned: externallyOwned, forceNewResource: false))
            {
                using (scopeUnderTest = new TestScope(anchor, resourceFactory, externallyOwned: externallyOwned, forceNewResource: true))
                {
                    observedCurrentScope = anchor.Current;
                    observedCurrentResource = anchor.Current.Resource;
                }

                observedDisposeCount = resourceCreatedByFactory[1].DisposeCount;
                observedPostCurrentScope = anchor.Current;
                observedPostCurrentResource = anchor.Current.Resource;
            }

            //-- assert

            anchor.Current.ShouldBe(null);

            observedCurrentScope.ShouldBeSameAs(scopeUnderTest);
            observedCurrentResource.ShouldBeSameAs(resourceCreatedByFactory[1]);

            observedPostCurrentScope.ShouldBeSameAs(outermostScope);
            observedPostCurrentResource.ShouldBeSameAs(resourceCreatedByFactory[0]);

            factoryCount.ShouldBe(2);
            observedDisposeCount.ShouldBe(externallyOwned ? 0 : 1);
            resourceCreatedByFactory[0].DisposeCount.ShouldBe(externallyOwned ? 0 : 1);

            CheckScopeChangedCount(resourceIsScopeHandler, resourceCreatedByFactory[0], 2);
            CheckScopeChangedCount(resourceIsScopeHandler, resourceCreatedByFactory[1], 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void FactoryThrowsException(
            [Values(true, false)] bool externallyOwned)
        {
            //-- arrange

            var factoryCount = 0;
            var anchor = new TestAnchor();

            TestScope scopeUnderTest = null;
            PerContextResourceConsumerScope<TestResource> observedCurrentScope = null;
            TestResource observedCurrentResource = null;

            //-- act

            Should.Throw<Exception>(
                () => {
                    using (scopeUnderTest = new TestScope(
                        anchor,
                        resourceFactory: h => {
                            factoryCount++;
                            throw new Exception();
                        },
                        externallyOwned: externallyOwned,
                        forceNewResource: false))
                    {
                        // We should not be here
                        observedCurrentScope = anchor.Current;
                        observedCurrentResource = anchor.Current.Resource;
                    }
                });

            //-- assert

            anchor.Current.ShouldBe(null);
            observedCurrentScope.ShouldBeSameAs(scopeUnderTest);    // null
            observedCurrentResource.ShouldBe(null);
            
            factoryCount.ShouldBe(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NestedOneLevelFactoryThrowsException(
            [Values(true, false)] bool externallyOwned,
            [Values(true, false)] bool resourceIsScopeHandler)
        {
            //-- arrange

            var factoryCount = 0;
            var anchor = new TestAnchor();

            TestResource resourceCreatedByFactory = null;

            Func<IResourceConsumerScopeHandle, TestResource> resourceFactory = h =>
            {
                var resource = resourceIsScopeHandler ? new ScopedTestResource() : new TestResource();
                resourceCreatedByFactory = resource;
                factoryCount++;
                return resource;
            };

            Func<IResourceConsumerScopeHandle, TestResource> exceptionResourceFactory = h => {
                throw new Exception();
            };

            PerContextResourceConsumerScope<TestResource> observedCurrentScope = null;
            TestResource observedCurrentResource = null;
            int observedDisposeCount;

            PerContextResourceConsumerScope<TestResource> observedPostCurrentScope;
            TestResource observedPostCurrentResource;

            TestScope outermostScope;
            TestScope scopeUnderTest = null;

            //-- act

            using (outermostScope = new TestScope(anchor, resourceFactory, externallyOwned: externallyOwned, forceNewResource: true))
            {
                Should.Throw<Exception>(
                    () => {
                        using (scopeUnderTest = new TestScope(anchor, exceptionResourceFactory, externallyOwned: externallyOwned, forceNewResource: true))
                        {
                            // We should not arrive here
                            observedCurrentScope = anchor.Current;
                            observedCurrentResource = anchor.Current.Resource;
                        }
                    });

                observedDisposeCount = resourceCreatedByFactory.DisposeCount;
                observedPostCurrentScope = anchor.Current;
                observedPostCurrentResource = anchor.Current.Resource;
            }

            //-- assert

            anchor.Current.ShouldBe(null);

            scopeUnderTest.ShouldBe(null);
            observedCurrentScope.ShouldBe(null);
            observedCurrentResource.ShouldBe(null);

            observedPostCurrentScope.ShouldBeSameAs(outermostScope);
            observedPostCurrentResource.ShouldBeSameAs(resourceCreatedByFactory);

            factoryCount.ShouldBe(1);
            observedDisposeCount.ShouldBe(0);
            resourceCreatedByFactory.DisposeCount.ShouldBe(externallyOwned ? 0 : 1);

            CheckScopeChangedCount(resourceIsScopeHandler, resourceCreatedByFactory, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CheckScopeChangedCount(bool resourceIsScopeHandler, TestResource resource, int numOFActivations)
        {
            ScopedTestResource scopedRresource = resource as ScopedTestResource;
            if (resourceIsScopeHandler)
            {
                scopedRresource.ShouldNotBe(null);
            }

            if (scopedRresource != null)
            {
                scopedRresource.ActiveScopeChangedCount.ShouldBe(numOFActivations);
                scopedRresource.DeactiveScopeChangedCount.ShouldBe(numOFActivations);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestScope : PerContextResourceConsumerScope<TestResource>
        {
            private readonly int _scopeId;

            public TestScope(
                TestAnchor anchor, 
                Func<IResourceConsumerScopeHandle, TestResource> resourceFactory, 
                bool externallyOwned, 
                bool forceNewResource)
                : base(anchor, resourceFactory, externallyOwned, forceNewResource)
            {
                _scopeId = ++LastScopeId;
            }

            public override string ToString()
            {
                return "Scope#" + _scopeId;
            }

            public static int LastScopeId { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestAnchor : ContextAnchor<PerContextResourceConsumerScope<TestResource>>
        {
            #region Overrides of ContextAnchor<T>

            public override void Clear()
            {
                Current = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override PerContextResourceConsumerScope<TestResource> Current { get; set; }

            #endregion
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestResource : IDisposable
        {
            private readonly int _resourceId;

            public TestResource()
            {
                _resourceId = ++LastResourceId;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                DisposeCount++;
            }

            #endregion

            public override string ToString()
            {
                return "Resource#" + _resourceId;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int DisposeCount { get; set; }

            public static int LastResourceId { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ScopedTestResource : TestResource, IScopedConsumptionResource
        {
            #region Implementation of IScopedConsumptionResource

            public void ActiveScopeChanged(bool currentScopeIsActive)
            {
                if (currentScopeIsActive)
                    ActiveScopeChangedCount++;
                else
                    DeactiveScopeChangedCount++;
            }

            #endregion

            public int ActiveScopeChangedCount { get; set; }
            public int DeactiveScopeChangedCount { get; set; }
        }
    }
}
