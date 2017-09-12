using System.Linq;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Runtime.Injection;
using Xunit;
using FluentAssertions;
using NWheels.Testability;
using System;

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
        public void CanResolveAll_NonGeneric()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();

            builder.RegisterComponentType<ComponentA>().ForService<IAnyComponent>();
            builder.RegisterComponentType<ComponentB>().ForService<IAnyComponent>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolved = containerUnderTest.ResolveAll(typeof(IAnyComponent)).ToArray();

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
        public void CanResolveComponent_NonGeneric()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();
            builder.RegisterComponentType<ComponentB>().ForService<IServiceB>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolvedA = containerUnderTest.Resolve(typeof(IServiceA));
            var resolvedB = containerUnderTest.Resolve(typeof(IServiceB));

            //-- assert

            resolvedA.Should().NotBeNull();
            resolvedA.Should().BeOfType<ComponentA>();

            resolvedB.Should().NotBeNull();
            resolvedB.Should().BeOfType<ComponentB>();
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
        public void CanResolveNamedComponent_NonGeneric()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().NamedForService<IAnyComponent>("AAA");
            builder.RegisterComponentType<ComponentB>().NamedForService<IAnyComponent>("BBB");

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var resolved = containerUnderTest.ResolveNamed(typeof(IAnyComponent), "BBB");

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
        public void TryResolveNonGeneric_ComponentRegistered_Resolved()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            object resolved;
            var result = containerUnderTest.TryResolve(typeof(IServiceA), out resolved);

            //-- assert

            result.Should().BeTrue();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentA>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TryResolveNonGeneric_ComponentNotRegistered_NullReturned()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            object resolved;
            var result = containerUnderTest.TryResolve(typeof(IServiceB), out resolved);

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

        [Fact]
        public void CanGetAllServiceTypes()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();
            builder.RegisterComponentType<ComponentB>().ForServices<IServiceB, IAnyComponent>();
            builder.RegisterComponentType<ComponentC>().NamedForServices<IServiceC, IAnyComponent>("CCC");

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);

            //-- act

            var allBasedOnObject = containerUnderTest.GetAllServiceTypes(typeof(object));
            var allBasedOnServiceC = containerUnderTest.GetAllServiceTypes(typeof(IServiceC));

            //-- assert

            allBasedOnObject.Should().BeEquivalentTo(
                typeof(Autofac.ILifetimeScope), typeof(Autofac.IComponentContext),
                typeof(IComponentContainer), typeof(IInternalComponentContainer),
                typeof(ComponentA), typeof(IServiceA), 
                typeof(ComponentB), typeof(IServiceB), 
                typeof(ComponentC), typeof(IServiceC), 
                typeof(IAnyComponent));

            allBasedOnServiceC.Should().BeEquivalentTo(
                typeof(ComponentC), typeof(IServiceC));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Dispose_DisposableComponentsAreDisposed()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentB>();

            var containerUnderTest = builder.CreateComponentContainer(isRootContainer: true);
            var component = containerUnderTest.Resolve<ComponentB>();

            //-- act

            var disposeCount0 = component.DisposeCount;

            containerUnderTest.Dispose();

            var disposeCount1 = component.DisposeCount;

            //-- assert

            disposeCount0.Should().Be(0);
            disposeCount1.Should().Be(1);
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
        public class ComponentB : IServiceB, ISpecialComponent, IAnyComponent, IDisposable
        {
            public void Dispose()
            {
                DisposeCount++;
            }
            public int DisposeCount { get; private set; }
        }
        public class ComponentC : IServiceC, ISpecialComponent, IAnyComponent
        {
        }
    }
}
