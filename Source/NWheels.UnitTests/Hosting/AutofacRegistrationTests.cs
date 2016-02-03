using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using NUnit.Framework;
using NWheels.Core;
using NWheels.Extensions;

namespace NWheels.UnitTests.Hosting
{
    [TestFixture]
    public class AutofacRegistrationTests
    {
        [Test]
        public void CanResolveByConventionThroughGenericHolder()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(Holder<>)).SingleInstance();

            var container = builder.Build();

            var holder1 = container.Resolve<Holder<BeingHoldOne>>();
            var holder2 = container.Resolve<Holder<BeingHoldTwo>>();

            Assert.That(holder1.Instance, Is.InstanceOf<BeingHoldOne>());
            Assert.That(holder2.Instance, Is.InstanceOf<BeingHoldTwo>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateLifetimeScopeAndUpdateMultipleTimes()
        {
            var baseBuilder = new ContainerBuilder();
            baseBuilder.RegisterType<ComponentZero>().As<IComponentZero>();
            var baseContainer = baseBuilder.Build();

            var lifetimeContainer = baseContainer.BeginLifetimeScope();

            var lifetimeUpdater1 = new ContainerBuilder();
            lifetimeUpdater1.RegisterType<ComponentOne>().As<IComponentOne>();
            lifetimeUpdater1.Update(lifetimeContainer.ComponentRegistry);

            var lifetimeUpdater2 = new ContainerBuilder();
            lifetimeUpdater2.RegisterType<ComponentTwo>().As<IComponentTwo>();
            lifetimeUpdater2.Update(lifetimeContainer.ComponentRegistry);

            var zero = lifetimeContainer.Resolve<IComponentZero>();
            var one = lifetimeContainer.Resolve<IComponentOne>();
            var two = lifetimeContainer.Resolve<IComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateLifetimeScopeAndRegisterMultipleModules()
        {
            //-- Arrange

            var baseBuilder = new ContainerBuilder();
            baseBuilder.RegisterType<ComponentZero>().As<IComponentZero>();
            var baseContainer = baseBuilder.Build();

            var lifetimeContainer = baseContainer.BeginLifetimeScope();

            //-- Act: Module One

            var lifetimeUpdater1a = new ContainerBuilder();
            lifetimeUpdater1a.RegisterType(typeof(TestModuleOne));
            lifetimeUpdater1a.Update(lifetimeContainer.ComponentRegistry);

            var moduleOne = (Module)lifetimeContainer.Resolve(typeof(TestModuleOne));

            var lifetimeUpdater1b = new ContainerBuilder();
            lifetimeUpdater1b.RegisterModule(moduleOne);
            lifetimeUpdater1b.Update(lifetimeContainer.ComponentRegistry);

            //-- Act: Module Two

            var lifetimeUpdater2a = new ContainerBuilder();
            lifetimeUpdater2a.RegisterType(typeof(TestModuleTwo));
            lifetimeUpdater2a.Update(lifetimeContainer.ComponentRegistry);

            var moduleTwo = (Module)lifetimeContainer.Resolve(typeof(TestModuleTwo));

            var lifetimeUpdater2b = new ContainerBuilder();
            lifetimeUpdater2b.RegisterModule(moduleTwo);
            lifetimeUpdater2b.Update(lifetimeContainer.ComponentRegistry);

            //-- Assert

            var zero = lifetimeContainer.Resolve<IComponentZero>();
            var one = lifetimeContainer.Resolve<IComponentOne>();
            var two = lifetimeContainer.Resolve<IComponentTwo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanInjectModuleComponentsIntoBaseContainerComponent_MustResolveFromChildContainer()
        {
            //-- Arrange

            var baseBuilder = new ContainerBuilder();
            baseBuilder.RegisterType<ComponentZero>().As<IComponentZero, IBaseline>();
            baseBuilder.RegisterType<BaselineSet>().InstancePerDependency();
            var baseContainer = baseBuilder.Build();

            var lifetimeContainer = baseContainer.BeginLifetimeScope();

            var lifetimeUpdater1a = new ContainerBuilder();
            lifetimeUpdater1a.RegisterType(typeof(TestModuleOne));
            lifetimeUpdater1a.Update(lifetimeContainer.ComponentRegistry);

            var moduleOne = (Module)lifetimeContainer.Resolve(typeof(TestModuleOne));

            var lifetimeUpdater1b = new ContainerBuilder();
            lifetimeUpdater1b.RegisterModule(moduleOne);
            lifetimeUpdater1b.Update(lifetimeContainer.ComponentRegistry);

            //-- Act: 

            var resolvedFromBase = baseContainer.Resolve<BaselineSet>();
            var resolvedFromChild = lifetimeContainer.Resolve<BaselineSet>();

            //-- Assert

            Assert.That(resolvedFromChild.Baselines.Length, Is.EqualTo(2));
            Assert.That(resolvedFromChild.Baselines.OfType<ComponentZero>().Count(), Is.EqualTo(1));
            Assert.That(resolvedFromChild.Baselines.OfType<ComponentOne>().Count(), Is.EqualTo(1));

            // injection from base container will only take components registered in the base container
            Assert.That(resolvedFromBase.Baselines.Length, Is.EqualTo(1));
            Assert.That(resolvedFromBase.Baselines.OfType<ComponentZero>().Count(), Is.EqualTo(1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanResolveComponentContextOfChildContainer()
        {
            //-- Arrange

            var baseBuilder = new ContainerBuilder();
            baseBuilder.RegisterType<ComponentZero>().As<IComponentZero, IBaseline>();
            var baseContainer = baseBuilder.Build();

            var lifetimeContainer = baseContainer.BeginLifetimeScope();

            var lifetimeUpdater1a = new ContainerBuilder();
            lifetimeUpdater1a.RegisterType(typeof(TestModuleOne));
            lifetimeUpdater1a.Update(lifetimeContainer.ComponentRegistry);

            var moduleOne = (Module)lifetimeContainer.Resolve(typeof(TestModuleOne));

            var lifetimeUpdater1b = new ContainerBuilder();
            lifetimeUpdater1b.RegisterModule(moduleOne);
            lifetimeUpdater1b.Update(lifetimeContainer.ComponentRegistry);

            //-- Act: 

            var componentContextFromBase = baseContainer.Resolve<IComponentContext>();
            var componentContextFromChild = lifetimeContainer.Resolve<IComponentContext>();

            //-- Assert

            Assert.That(componentContextFromBase.ComponentRegistry.Registrations.Count(), Is.EqualTo(2));
            Assert.That(componentContextFromChild.ComponentRegistry.Registrations.Count(), Is.EqualTo(3));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanDetermineIfComponentIsRegisteredAsSingleton()
        {
            //-- Arrange

            var builder = new ContainerBuilder();

            builder.RegisterType<ComponentOne>().As<IComponentOne>().SingleInstance();
            builder.RegisterType<ComponentTwo>().As<IComponentTwo>().InstancePerDependency();

            var container = builder.Build();

            //-- Act: 

            IComponentRegistration registrationOne;
            IComponentRegistration registrationTwo;

            if ( !container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(IComponentOne)), out registrationOne) )
            {
                Assert.Fail("Registration for IComponentOne could not be retrieved");
            }

            if ( !container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(IComponentTwo)), out registrationTwo) )
            {
                Assert.Fail("Registration for IComponentTwo could not be retrieved");
            }

            var isOneSingleton = (registrationOne.Sharing == InstanceSharing.Shared && registrationOne.Lifetime is RootScopeLifetime);
            var isTwoSingleton = (registrationTwo.Sharing == InstanceSharing.Shared && registrationOne.Lifetime is RootScopeLifetime);

            //-- Assert

            Assert.That(isOneSingleton, Is.True);
            Assert.That(isTwoSingleton, Is.False);

            Assert.That(registrationOne.Lifetime, Is.InstanceOf<RootScopeLifetime>());
            Assert.That(registrationOne.Sharing, Is.EqualTo(InstanceSharing.Shared));

            Assert.That(registrationTwo.Lifetime, Is.InstanceOf<CurrentScopeLifetime>());
            Assert.That(registrationTwo.Sharing, Is.EqualTo(InstanceSharing.None));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Test, ExpectedException(exc = "A am here!")]
        //public void CanResolveByBaseTypeConvention()
        //{
        //    var builder = new ContainerBuilder();

        //    builder.Register<Func<IBeingHold>>(
        //        c => {
        //            throw new Exception("I am here!");
        //        });
            
        //    var container = builder.Build();

        //    var obj = container.Resolve<Func<IBeingHold>>();
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Holder<T> where T : IBeingHold, new()
        {
            public Holder()
            {
                this.Instance = new T();
            }
            public T Instance { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBeingHold
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBeingHoldDerived : IBeingHold
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BeingHoldOne : IBeingHold
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BeingHoldTwo : IBeingHold
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IBaseline
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BaselineSet
        {
            public BaselineSet(IEnumerable<IBaseline> baselines)
            {
                this.Baselines = baselines.ToArray();
            }
            public IBaseline[] Baselines { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IComponentZero : IBaseline
        {
        }
        public class ComponentZero : IComponentZero
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IComponentOne : IBaseline
        {
        }
        public class ComponentOne : IComponentOne
        {
        }
        public class TestModuleOne : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ComponentOne>().As<IComponentOne, IBaseline>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IComponentTwo : IBaseline
        {
        }
        public class ComponentTwo : IComponentTwo
        {
        }
        public class TestModuleTwo : Module
        {
            public TestModuleTwo(IComponentZero zero)
            {
            }
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ComponentTwo>().As<IComponentTwo, IBaseline>();
            }
        }
    }
}
