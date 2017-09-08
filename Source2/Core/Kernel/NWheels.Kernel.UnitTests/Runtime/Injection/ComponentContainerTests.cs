using System.Linq;
using NWheels.Injection;
using NWheels.Runtime.Injection;
using Xunit;
using FluentAssertions;

namespace NWheels.Kernel.UnitTests.Runtime.Injection
{
    public class ComponentContainerTests
    {
        [Fact]
        public void CanResolveAll()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();

            builder.RegisterComponentType<ComponentA>().ForService<IAnyComponent>();
            builder.RegisterComponentType<ComponentB>().ForService<IAnyComponent>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolved = containerUnderTest.ResolveAll<IAnyComponent>().ToArray();

            //-- assert

            resolved.Length.Should().Be(2);
            resolved.OfType<ComponentA>().Count().Should().Be(1);
            resolved.OfType<ComponentB>().Count().Should().Be(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanResolveSelf()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var container = containerUnderTest.Resolve<IComponentContainer>();
            var internalContainer = containerUnderTest.Resolve<IInternalComponentContainer>();

            //-- assert

            container.Should().NotBeNull();
            internalContainer.Should().BeSameAs(container);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Merge_Resolve_ReturnsLastComponentRegisteredForService()
        {
            //-- arrange

            var builder1 = new ComponentContainerBuilder();
            builder1.RegisterComponentType<ComponentA>().ForService<IAnyComponent>();

            var containerUnderTest = builder1.CreateComponentContainer(isRootContainer: true);

            //-- act

            var builder2 = new ComponentContainerBuilder();
            builder2.RegisterComponentType<ComponentB>().ForService<IAnyComponent>();
            containerUnderTest.Merge(builder2);

            //-- assert

            var resolved = containerUnderTest.Resolve<IAnyComponent>();

            resolved.Should().NotBeNull();
            resolved.Should().BeAssignableTo<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Merge_ResolveAll_ReturnsAllComponentsRegisteredForService()
        {
            //-- arrange

            var builder1 = new ComponentContainerBuilder();
            builder1.RegisterComponentType<ComponentA>().ForService<IAnyComponent>();

            var containerUnderTest = builder1.CreateComponentContainer(isRootContainer: true);

            //-- act

            var builder2 = new ComponentContainerBuilder();
            builder2.RegisterComponentType<ComponentB>().ForService<IAnyComponent>();
            containerUnderTest.Merge(builder2);

            //-- assert

            var resolved = containerUnderTest.ResolveAll<IAnyComponent>().ToArray();

            resolved.Length.Should().Be(2);
            resolved.OfType<ComponentA>().Count().Should().Be(1);
            resolved.OfType<ComponentB>().Count().Should().Be(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAnyComponent
        {
        }
        public class ComponentA : IAnyComponent
        {
        }
        public class ComponentB : IAnyComponent
        {
        }
    }
}
