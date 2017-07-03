using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Implementation.UnitTests
{
    public class LazySlimTests
    {
        [Fact]
        public void CanInitializeWithValue()
        {
            //-- arrange

            var value = new object();
            var lazyUnderTest = new LazySlim<object>(value);

            //-- act

            var actualValue1 = lazyUnderTest.Value;
            var actualValue2 = lazyUnderTest.Value;

            //-- assert

            actualValue1.Should().BeSameAs(value);
            actualValue2.Should().BeSameAs(value);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInitializeWithFactory()
        {
            //-- arrange

            var value = new object();
            var factoryCallCount = 0;
            Func<object> factory = () => {
                factoryCallCount++;
                return value;
            };

            var lazyUnderTest = new LazySlim<object>(factory);

            //-- act

            var actualFactoryCallCount0 = factoryCallCount;

            var actualValue1 = lazyUnderTest.Value;
            var actualFactoryCallCount1 = factoryCallCount;

            var actualValue2 = lazyUnderTest.Value;
            var actualFactoryCallCount2 = factoryCallCount;

            //-- assert

            actualValue1.Should().BeSameAs(value);
            actualValue2.Should().BeSameAs(value);

            actualFactoryCallCount0.Should().Be(0);
            actualFactoryCallCount1.Should().Be(1);
            actualFactoryCallCount2.Should().Be(1);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanHandleDefaultValues()
        {
            //-- arrange

            var factoryCallCount = 0;
            Func<int> factory = () => {
                factoryCallCount++;
                return 0;
            };

            var lazyUnderTest = new LazySlim<int>(factory);

            //-- act

            var actualFactoryCallCount0 = factoryCallCount;

            var actualValue1 = lazyUnderTest.Value;
            var actualFactoryCallCount1 = factoryCallCount;

            var actualValue2 = lazyUnderTest.Value;
            var actualFactoryCallCount2 = factoryCallCount;

            //-- assert

            actualValue1.Should().Be(0);
            actualValue2.Should().Be(0);

            actualFactoryCallCount0.Should().Be(0);
            actualFactoryCallCount1.Should().Be(1);
            actualFactoryCallCount2.Should().Be(1);
        }
    }
}
