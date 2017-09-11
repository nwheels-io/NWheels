using System.Linq;
using NWheels.Injection;
using NWheels.Runtime.Injection;
using Xunit;
using FluentAssertions;
using NWheels.Testability;

namespace NWheels.Kernel.UnitTests.Runtime.Injection
{
    public class ComponentContainerTests : TestBase.UnitTest
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
        public void CanResolveComponentByConcreteType()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>();
            builder.RegisterComponentType<ComponentB>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve<ComponentA>();
            var resolvedB = containerUnderTest.Resolve<ComponentB>();

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanResolveComponentByService()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();
            builder.RegisterComponentType<ComponentB>().ForService<IServiceB>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve<IServiceA>();
            var resolvedB = containerUnderTest.Resolve<IServiceB>();

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForTwoServices()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForServices<IServiceA, IAnyComponent>();
            builder.RegisterComponentType<ComponentB>().ForServices<IServiceB, IAnyComponent>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve<IServiceA>();
            var resolvedB = containerUnderTest.Resolve<IServiceB>();
            var resolvedAny = containerUnderTest.ResolveAll<IAnyComponent>().ToArray();

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();

            resolvedAny.Should().NotBeNull();
            resolvedAny.Select(r => r.GetType()).Should().BeEquivalentTo(typeof(ComponentA), typeof(ComponentB));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForThreeServices()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForServices<IServiceA, IAnyComponent>();
            builder.RegisterComponentType<ComponentB>().ForServices<IServiceB, ISpecialComponent, IAnyComponent>();
            builder.RegisterComponentType<ComponentC>().ForServices<IServiceC, ISpecialComponent, IAnyComponent>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve<IServiceA>();
            var resolvedB = containerUnderTest.Resolve<IServiceB>();
            var resolvedSpecial = containerUnderTest.ResolveAll<ISpecialComponent>().ToArray();

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();

            resolvedSpecial.Should().NotBeNull();
            resolvedSpecial.Select(r => r.GetType()).Should().BeEquivalentTo(typeof(ComponentB), typeof(ComponentC));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForMultipleServices_NonGeneric()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForServices(typeof(IServiceA), typeof(IAnyComponent));
            builder.RegisterComponentType<ComponentB>().ForServices(typeof(IServiceB), typeof(IAnyComponent));

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve<IServiceA>();
            var resolvedB = containerUnderTest.Resolve<IServiceB>();
            var resolvedAny = containerUnderTest.ResolveAll<IAnyComponent>().ToArray();

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();

            resolvedAny.Should().NotBeNull();
            resolvedAny.Select(r => r.GetType()).Should().BeEquivalentTo(typeof(ComponentA), typeof(ComponentB));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentType_NonGeneric()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType(typeof(ComponentA)).ForService<IServiceA>();
            builder.RegisterComponentType(typeof(ComponentB)).ForService<IServiceB>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve<IServiceA>();
            var resolvedB = containerUnderTest.Resolve<IServiceB>();

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterNamedComponentForTwoServices()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentB>().NamedForServices<ISpecialComponent, IAnyComponent>("BBB");
            builder.RegisterComponentType<ComponentC>().NamedForServices<ISpecialComponent, IAnyComponent>("CCC");

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedB = containerUnderTest.ResolveNamed<ISpecialComponent>("BBB");
            var resolvedC = containerUnderTest.ResolveNamed<IAnyComponent>("CCC");

            //-- assert

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();

            resolvedC.Should().NotBeNull();
            resolvedC.Should().BeOfType<ComponentC>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterNamedComponentForThreeServices()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentB>().NamedForServices<IServiceB, ISpecialComponent, IAnyComponent>("BBB");
            builder.RegisterComponentType<ComponentC>().NamedForServices<IServiceC, ISpecialComponent, IAnyComponent>("CCC");

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedB = containerUnderTest.ResolveNamed<IServiceB>("BBB");
            var resolvedC = containerUnderTest.ResolveNamed<ISpecialComponent>("CCC");

            //-- assert

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();

            resolvedC.Should().NotBeNull();
            resolvedC.Should().BeOfType<ComponentC>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterNamedComponentForMultipleServices_NonGeneric()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentB>().NamedForServices("BBB", typeof(ISpecialComponent), typeof(IAnyComponent));
            builder.RegisterComponentType<ComponentC>().NamedForServices("CCC", typeof(ISpecialComponent), typeof(IAnyComponent));

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedB = containerUnderTest.ResolveNamed<ISpecialComponent>("BBB");
            var resolvedC = containerUnderTest.ResolveNamed<IAnyComponent>("CCC");

            //-- assert

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();

            resolvedC.Should().NotBeNull();
            resolvedC.Should().BeOfType<ComponentC>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RegisterComponentAsFallback_NoOverrideRegistered_FallbackResolved()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentB>().ForService<ISpecialComponent>().AsFallback();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolved = containerUnderTest.Resolve<ISpecialComponent>();

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RegisterComponentAsFallback_OverrideRegistered_OverrideResolved()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentB>().ForService<ISpecialComponent>().AsFallback();
            builder.RegisterComponentType<ComponentC>().ForService<ISpecialComponent>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolved = containerUnderTest.Resolve<ISpecialComponent>();

            //-- assert

            resolved.Should().BeOfType<ComponentC>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanResolveNamedComponent()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().NamedForService<IAnyComponent>("AAA");
            builder.RegisterComponentType<ComponentB>().NamedForService<IAnyComponent>("BBB");

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolved = containerUnderTest.ResolveNamed<IAnyComponent>("BBB");

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TryResolve_ComponentRegistered_Resolved()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            IServiceA resolved;
            var result = containerUnderTest.TryResolve<IServiceA>(out resolved);

            //-- assert

            result.Should().BeTrue();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentA>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TryResolve_ComponentNotRegistered_NullReturned()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            IServiceB resolved;
            var result = containerUnderTest.TryResolve<IServiceB>(out resolved);

            //-- assert

            result.Should().BeFalse();
            resolved.Should().BeNull();
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
        public interface ISpecialComponent
        {
        }
        public interface IServiceA
        {
        }
        public interface IServiceB
        {
        }
        public interface IServiceC
        {
        }
        public class ComponentA : IServiceA, IAnyComponent
        {
        }
        public class ComponentB : IServiceB, ISpecialComponent, IAnyComponent
        {
        }
        public class ComponentC : IServiceC, ISpecialComponent, IAnyComponent
        {
        }
    }
}
