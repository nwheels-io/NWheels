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
    }
}
