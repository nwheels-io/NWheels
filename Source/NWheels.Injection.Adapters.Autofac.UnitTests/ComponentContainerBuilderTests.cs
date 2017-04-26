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
        public void CanRegisterSingletonComponentType()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().SingleInstance();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var resolved1 = container.Resolve<ComponentOne>();
            var resolved2 = container.Resolve<ComponentOne>();

            resolved2.Should().BeSameAs(resolved1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterTransientComponentType()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>(); // no need to specify - default is InstancePerDependency
            builderUnderTest.RegisterComponentType<ComponentTwo>().InstancePerDependency();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);

            var one1 = container.Resolve<ComponentOne>();
            var one2 = container.Resolve<ComponentOne>();

            var two1 = container.Resolve<ComponentTwo>();
            var two2 = container.Resolve<ComponentTwo>();

            one2.Should().NotBeSameAs(one1);
            two2.Should().NotBeSameAs(two1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentTypeNonGeneric()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType(typeof(ComponentOne))
                .SingleInstance()
                .ForService<IServiceOne>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var asService = container.Resolve<IServiceOne>();
            var asSelf = container.Resolve<ComponentOne>();

            asSelf.Should().NotBeNull();
            asService.Should().BeSameAs(asSelf);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentTypeForOneService()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().ForService<IServiceOne>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var asService = container.Resolve<IServiceOne>();
            var asSelf = container.Resolve<ComponentOne>();

            asService.Should().NotBeNull();
            asService.Should().BeAssignableTo<ComponentOne>();
            asSelf.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentTypeForTwoServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().ForServices<IServiceOne, ITestComponent>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
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
        public void CanRegisterComponentTypeForThreeServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().ForServices<IServiceOne, ITestComponent, IAnyComponent>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
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
        public void CanRegisterComponentTypeForArrayOfServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().ForServices(typeof(IServiceOne), typeof(IAnyComponent));

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
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

            builderUnderTest.RegisterComponentType<ComponentOne>().SingleInstance();
            builderUnderTest.RegisterComponentType<ComponentThree>().WithParameter<int>(123);

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var resolved = container.Resolve<ComponentThree>();

            resolved.Should().NotBeNull();
            resolved.Number.Should().Be(123);
            resolved.One.Should().BeSameAs(container.Resolve<ComponentOne>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentInstance()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();
            var componentInstance = new ComponentOne();

            //-- act

            builderUnderTest.RegisterComponentInstance<ComponentOne>(componentInstance);

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var resolved = container.Resolve<ComponentOne>();

            resolved.Should().NotBeNull();
            resolved.Should().BeSameAs(componentInstance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentInstanceForServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();
            var componentInstance = new ComponentOne();

            //-- act

            builderUnderTest.RegisterComponentInstance<ComponentOne>(componentInstance).ForServices<IServiceOne, IAnyComponent>();

            //-- assert

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
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

            builderUnderTest.RegisterComponentType<ComponentFour>();

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var resolved = container.Resolve<ComponentFour>();

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Container.Should().NotBeNull();
            resolved.Container.Should().BeSameAs(container);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterFallbackComponent()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().ForService<ITestComponent>();
            builderUnderTest.RegisterComponentType<ComponentTwo>().ForService<ITestComponent>().AsFallback();


            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);
            var resolved = container.Resolve<ITestComponent>();

            //-- assert

            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<ComponentOne>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentNamedForService()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().NamedForService<ITestComponent>("A");
            builderUnderTest.RegisterComponentType<ComponentTwo>().NamedForService<ITestComponent>("B");


            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);

            var resolved1 = container.ResolveNamed<ITestComponent>("A");
            var resolved2 = container.ResolveNamed<ITestComponent>("B");

            //-- assert

            resolved1.Should().BeOfType<ComponentOne>();
            resolved2.Should().BeOfType<ComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentNamedForTwoServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            //-- act

            builderUnderTest.RegisterComponentType<ComponentOne>().NamedForServices<ITestComponent, IAnyComponent>("A");
            builderUnderTest.RegisterComponentType<ComponentTwo>().NamedForServices<ITestComponent, IAnyComponent>("B");


            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);

            var resolved1a = container.ResolveNamed<IAnyComponent>("A");
            var resolved1b = container.ResolveNamed<ITestComponent>("A");

            var resolved2a = container.ResolveNamed<IAnyComponent>("B");
            var resolved2b = container.ResolveNamed<ITestComponent>("B");

            //-- assert

            resolved1a.Should().BeOfType<ComponentOne>();
            resolved1b.Should().BeOfType<ComponentOne>();

            resolved2a.Should().BeOfType<ComponentTwo>();
            resolved2b.Should().BeOfType<ComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentNamedForThreeServices()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            var instanceA = new ComponentOne();
            var instanceB = new ComponentOne();

            //-- act

            builderUnderTest.RegisterComponentInstance(instanceA).NamedForServices<IServiceOne, ITestComponent, IAnyComponent>("A");
            builderUnderTest.RegisterComponentInstance(instanceB).NamedForServices<IServiceOne, ITestComponent, IAnyComponent>("B");

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);

            var resolvedA1 = container.ResolveNamed<IServiceOne>("A");
            var resolvedA2 = container.ResolveNamed<IAnyComponent>("A");
            var resolvedA3 = container.ResolveNamed<ITestComponent>("A");

            var resolvedB1 = container.ResolveNamed<IServiceOne>("B");
            var resolvedB2 = container.ResolveNamed<IAnyComponent>("B");
            var resolvedB3 = container.ResolveNamed<ITestComponent>("B");

            //-- assert

            resolvedA1.Should().BeSameAs(instanceA);
            resolvedA2.Should().BeSameAs(instanceA);
            resolvedA3.Should().BeSameAs(instanceA);

            resolvedB1.Should().BeSameAs(instanceB);
            resolvedB2.Should().BeSameAs(instanceB);
            resolvedB3.Should().BeSameAs(instanceB);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRegisterComponentNamedForMultipleServicesNonGeneric()
        {
            //-- arrange

            IComponentContainerBuilder builderUnderTest = new ComponentContainerBuilder();

            var instanceA = new ComponentOne();
            var instanceB = new ComponentOne();

            //-- act

            builderUnderTest.RegisterComponentInstance(instanceA).NamedForServices("A", typeof(IServiceOne), typeof(ITestComponent));
            builderUnderTest.RegisterComponentInstance(instanceB).NamedForServices("B", typeof(IServiceOne), typeof(ITestComponent));

            var container = ((IInternalComponentContainerBuilder)builderUnderTest).CreateComponentContainer(isRootContainer: true);

            var resolvedA1 = container.ResolveNamed<IServiceOne>("A");
            var resolvedA2 = container.ResolveNamed<ITestComponent>("A");

            var resolvedB1 = container.ResolveNamed<IServiceOne>("B");
            var resolvedB2 = container.ResolveNamed<ITestComponent>("B");

            //-- assert

            resolvedA1.Should().BeSameAs(instanceA);
            resolvedA2.Should().BeSameAs(instanceA);

            resolvedB1.Should().BeSameAs(instanceB);
            resolvedB2.Should().BeSameAs(instanceB);
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
