using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Injection.Adapters.Autofac.UnitTests
{
    public class ComponentContainerBuilderTests
    {
        [Fact]
        public void CanRegisterSingletonComponent()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>().SingleInstance();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var resolved1 = container.Resolve<ComponentOne>();
            var resolved2 = container.Resolve<ComponentOne>();

            resolved2.Should().BeSameAs(resolved1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterTransientComponent()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>(); // no need to specify - default is InstancePerDependency
            builderUnderTest.RegisterComponent2<ComponentTwo>().InstancePerDependency();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();

            var one1 = container.Resolve<ComponentOne>();
            var one2 = container.Resolve<ComponentOne>();

            var two1 = container.Resolve<ComponentTwo>();
            var two2 = container.Resolve<ComponentTwo>();

            one2.Should().NotBeSameAs(one1);
            two2.Should().NotBeSameAs(two1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForOneService()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>().As<IServiceOne>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var asService = container.Resolve<IServiceOne>();
            var asSelf = container.Resolve<ComponentOne>();

            asService.Should().NotBeNull();
            asService.Should().BeAssignableTo<ComponentOne>();
            asSelf.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForTwoServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>().As<IServiceOne, ITestComponent>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var asService1 = container.Resolve<IServiceOne>();
            var asService2 = container.Resolve<ITestComponent>();
            var asSelf = container.Resolve<ComponentOne>();

            asService1.Should().NotBeNull();
            asService1.Should().BeAssignableTo<ComponentOne>();

            asService2.Should().NotBeNull();
            asService2.Should().BeAssignableTo<ComponentOne>();

            asSelf.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForThreeServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>().As<IServiceOne, ITestComponent, IAnyComponent>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var asService1 = container.Resolve<IServiceOne>();
            var asService2 = container.Resolve<ITestComponent>();
            var asService3 = container.Resolve<IAnyComponent>();
            var asSelf = container.Resolve<ComponentOne>();

            asService1.Should().NotBeNull();
            asService1.Should().BeAssignableTo<ComponentOne>();

            asService2.Should().NotBeNull();
            asService2.Should().BeAssignableTo<ComponentOne>();

            asService3.Should().NotBeNull();
            asService3.Should().BeAssignableTo<ComponentOne>();

            asSelf.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentForArrayOfServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>().As(typeof(IServiceOne), typeof(IAnyComponent));

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var asService1 = container.Resolve<IServiceOne>();
            var asService2 = container.Resolve<IAnyComponent>();
            var asSelf = container.Resolve<ComponentOne>();

            asService1.Should().NotBeNull();
            asService1.Should().BeAssignableTo<ComponentOne>();

            asService2.Should().NotBeNull();
            asService2.Should().BeAssignableTo<ComponentOne>();

            asSelf.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanPassConstructorParameterByType()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentOne>().SingleInstance();
            builderUnderTest.RegisterComponent2<ComponentThree>().WithParameter<int>(123);

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var resolved = container.Resolve<ComponentThree>();

            resolved.Should().NotBeNull();
            resolved.Number.Should().Be(123);
            resolved.One.Should().BeSameAs(container.Resolve<ComponentOne>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterInstance()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();
            var componentInstance = new ComponentOne();

            //-- act

            builderUnderTest.RegisterInstance2<ComponentOne>(componentInstance);

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var resolved = container.Resolve<ComponentOne>();

            resolved.Should().NotBeNull();
            resolved.Should().BeSameAs(componentInstance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterInstanceForServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();
            var componentInstance = new ComponentOne();

            //-- act

            builderUnderTest.RegisterInstance2<ComponentOne>(componentInstance).As<IServiceOne, IAnyComponent>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var asService1 = container.Resolve<IServiceOne>();
            var asService2 = container.Resolve<IAnyComponent>();
            var asSelf = container.Resolve<ComponentOne>();

            asService1.Should().BeSameAs(componentInstance);
            asService2.Should().BeSameAs(componentInstance);
            asSelf.Should().BeSameAs(componentInstance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInjectContainerAsDependency()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponent2<ComponentFour>();

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer();
            var resolved = container.Resolve<ComponentFour>();

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Container.Should().NotBeNull();
            resolved.Container.Should().BeSameAs(container);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAnyComponent
        {
        }
        public interface ITestComponent
        {
        }
        public interface IServiceOne
        {
        }
        public interface IServiceTwo
        {
        }
        public class ComponentOne : IServiceOne, ITestComponent, IAnyComponent
        {
        }
        public class ComponentTwo : IServiceTwo, ITestComponent, IAnyComponent
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ComponentThree : IAnyComponent
        {
            public ComponentThree(ComponentOne one, int number)
            {
                this.One = one;
                this.Number = number;
            }
            public ComponentOne One { get; }
            public int Number { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ComponentFour : IAnyComponent
        {
            public ComponentFour(IComponentContainer container)
            {
                this.Container = container;
            }
            public IComponentContainer Container { get; }
        }
    }
}
