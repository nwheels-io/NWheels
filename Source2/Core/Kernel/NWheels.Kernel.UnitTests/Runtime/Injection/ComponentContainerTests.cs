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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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
            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveNamed(typeof(IAnyComponent), "BBB");

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentB>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveWithArguments_One()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentAWithParam>().ForService<IServiceA>();

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveWithArguments<IServiceA, TimeSpan>(TimeSpan.FromSeconds(123));

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithParam>();
            
            ((ComponentAWithParam)resolved).Value.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveWithArguments_Two()
        {
            //-- arrange

            var componentB = new ComponentB();

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentAWithManyParams>().SingleInstance().ForService<IServiceA>();
            builder.RegisterComponentInstance(componentB).ForService<IServiceB>();

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveWithArguments<IServiceA, TimeSpan, DayOfWeek>(TimeSpan.FromSeconds(123), DayOfWeek.Tuesday);

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithManyParams>();
            
            ((ComponentAWithManyParams)resolved).ServiceB.Should().BeSameAs(componentB);
            ((ComponentAWithManyParams)resolved).DayValue.Should().Be(DayOfWeek.Tuesday);
            ((ComponentAWithManyParams)resolved).TimeValue.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveWithArguments_Array()
        {
            //-- arrange

            var componentB = new ComponentB();

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentAWithManyParams>().SingleInstance().ForService<IServiceA>();
            builder.RegisterComponentInstance(componentB).ForService<IServiceB>();

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveWithArguments<IServiceA>(TimeSpan.FromSeconds(123), DayOfWeek.Tuesday);

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithManyParams>();
            
            ((ComponentAWithManyParams)resolved).ServiceB.Should().BeSameAs(componentB);
            ((ComponentAWithManyParams)resolved).DayValue.Should().Be(DayOfWeek.Tuesday);
            ((ComponentAWithManyParams)resolved).TimeValue.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveWithArguments_NonGeneric()
        {
            //-- arrange

            var componentB = new ComponentB();

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentAWithManyParams>().SingleInstance().ForService<IServiceA>();
            builder.RegisterComponentInstance(componentB).ForService<IServiceB>();

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveWithArguments(typeof(IServiceA), TimeSpan.FromSeconds(123), DayOfWeek.Tuesday);

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithManyParams>();
            
            ((ComponentAWithManyParams)resolved).ServiceB.Should().BeSameAs(componentB);
            ((ComponentAWithManyParams)resolved).DayValue.Should().Be(DayOfWeek.Tuesday);
            ((ComponentAWithManyParams)resolved).TimeValue.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveNamedWithArguments_One()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("X");
            builder.RegisterComponentType<ComponentAWithParam>().NamedForService<IServiceA>("Y");
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("Z");

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveNamedWithArguments<IServiceA, TimeSpan>("Y", TimeSpan.FromSeconds(123));

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithParam>();
            
            ((ComponentAWithParam)resolved).Value.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveNamedWithArguments_Two()
        {
            //-- arrange

            var componentB = new ComponentB();

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentInstance(componentB).ForService<IServiceB>();
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("X");
            builder.RegisterComponentType<ComponentAWithManyParams>().NamedForService<IServiceA>("Y");
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("Z");

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveNamedWithArguments<IServiceA, TimeSpan, DayOfWeek>("Y", TimeSpan.FromSeconds(123), DayOfWeek.Tuesday);

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithManyParams>();
            
            ((ComponentAWithManyParams)resolved).ServiceB.Should().BeSameAs(componentB);
            ((ComponentAWithManyParams)resolved).DayValue.Should().Be(DayOfWeek.Tuesday);
            ((ComponentAWithManyParams)resolved).TimeValue.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveNamedWithArguments_Array()
        {
            //-- arrange

            var componentB = new ComponentB();

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentInstance(componentB).ForService<IServiceB>();
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("X");
            builder.RegisterComponentType<ComponentAWithManyParams>().NamedForService<IServiceA>("Y");
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("Z");

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveNamedWithArguments<IServiceA>("Y", TimeSpan.FromSeconds(123), DayOfWeek.Tuesday);

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithManyParams>();
            
            ((ComponentAWithManyParams)resolved).ServiceB.Should().BeSameAs(componentB);
            ((ComponentAWithManyParams)resolved).DayValue.Should().Be(DayOfWeek.Tuesday);
            ((ComponentAWithManyParams)resolved).TimeValue.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ResolveNamedWithArguments_NonGeneric()
        {
            //-- arrange

            var componentB = new ComponentB();

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentInstance(componentB).ForService<IServiceB>();
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("X");
            builder.RegisterComponentType<ComponentAWithManyParams>().NamedForService<IServiceA>("Y");
            builder.RegisterComponentType<ComponentA>().NamedForService<IServiceA>("Z");

            var containerUnderTest = builder.CreateComponentContainer();

            //-- act

            var resolved = containerUnderTest.ResolveNamedWithArguments(typeof(IServiceA), "Y", TimeSpan.FromSeconds(123), DayOfWeek.Tuesday);

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentAWithManyParams>();
            
            ((ComponentAWithManyParams)resolved).ServiceB.Should().BeSameAs(componentB);
            ((ComponentAWithManyParams)resolved).DayValue.Should().Be(DayOfWeek.Tuesday);
            ((ComponentAWithManyParams)resolved).TimeValue.Should().Be(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TryResolve_ComponentRegistered_Resolved()
        {
            //-- arrange

            var builder = new ComponentContainerBuilder();
            builder.RegisterComponentType<ComponentA>().ForService<IServiceA>();

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder1.CreateComponentContainer();

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

            var containerUnderTest = builder1.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();

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

            var containerUnderTest = builder.CreateComponentContainer();
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
        public class ComponentAWithParam : IServiceA
        {
            public ComponentAWithParam(TimeSpan value)
            {
                Value = value;
            }
            public TimeSpan Value { get; }
        }
        public class ComponentAWithManyParams : IServiceA
        {
            public ComponentAWithManyParams(IServiceB serviceB, DayOfWeek dayValue, TimeSpan timeValue)
            {
                this.ServiceB = serviceB;
                this.DayValue = dayValue;
                this.TimeValue = timeValue;
            }

            public IServiceB ServiceB { get; }
            public DayOfWeek DayValue { get; }
            public TimeSpan TimeValue { get; }
        }
    }
}
