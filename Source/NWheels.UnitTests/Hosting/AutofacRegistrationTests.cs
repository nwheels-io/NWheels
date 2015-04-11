using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using NUnit.Framework;

namespace NWheels.Core.UnitTests.Hosting
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

        public interface IComponentZero
        {
        }
        public class ComponentZero : IComponentZero
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IComponentOne
        {
        }
        public class ComponentOne : IComponentOne
        {
        }
        public class TestModuleOne : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ComponentOne>().As<IComponentOne>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IComponentTwo
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
                builder.RegisterType<ComponentTwo>().As<IComponentTwo>();
            }
        }
    }
}
