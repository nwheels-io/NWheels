using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Conventions;

namespace NWheels.Core.UnitTests
{
    [TestFixture]
    public class AutoTests
    {
        [Test]
        public void CanRegisterAutoFactoryForAuto()
        {
            //-- Arrange

            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(Auto<>)).SingleInstance();
            builder.RegisterType<TestAutoFactoryOne>().As<IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<TestAutoFactoryTwo>().As<IAutoObjectFactory>().SingleInstance();
            builder.RegisterType<AConsumerComponent>();

            var container = builder.Build();

            //-- Act 

            var one = container.Resolve<Auto<IContractOne>>().Instance;
            var two = container.Resolve<Auto<IContractTwo>>().Instance;
            var consumer = container.Resolve<AConsumerComponent>();

            //-- Assert

            Assert.That(one.Value, Is.EqualTo(123));
            Assert.That(two.Value, Is.EqualTo("ABC"));
            Assert.That(consumer.ValueOne, Is.EqualTo(123));
            Assert.That(consumer.ValueTwo, Is.EqualTo("ABC"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanImplicitlyCastServiceImlementorToAuto()
        {
            //-- Arrange

            var testFunction = new Func<Auto<IContractOne>, int>(a => a.Instance.Value);
            var one = new ImplOne() { Value = 123 };

            //-- Act

            var value = testFunction(one);

            //-- Assert

            Assert.That(value, Is.EqualTo(123));
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanNotImplicitlyCastServiceInterfaceToAuto()
        {
            //-- Arrange

            var testFunction = new Func<Auto<IContractOne>, int>(a => a.Instance.Value);
            IContractOne one = new ImplOne() { Value = 123 };

            //-- Act

            var value = testFunction(Auto.Of(one));

            //-- Assert

            Assert.That(value, Is.EqualTo(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AConsumerComponent
        {
            public AConsumerComponent(Auto<IContractOne> one, Auto<IContractTwo> two)
            {
                ValueOne = one.Instance.Value;
                ValueTwo = two.Instance.Value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int ValueOne { get; set; }
            public string ValueTwo { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestAutoFactoryOne : IAutoObjectFactory
        {
            public TService CreateService<TService>() where TService : class
            {
                if ( typeof(TService) == typeof(IContractOne) )
                {
                    return (TService)(object)(new ImplOne {
                        Value = 123
                    });
                }

                throw new NotSupportedException(typeof(TService).Name);
            }

            public Type ServiceAncestorMarkerType
            {
                get { return typeof(IContractMarkerOne); }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestAutoFactoryTwo : IAutoObjectFactory
        {
            public TService CreateService<TService>() where TService : class
            {
                if ( typeof(TService) == typeof(IContractTwo) )
                {
                    return (TService)(object)(new ImplTwo {
                        Value = "ABC"
                    });
                }

                throw new NotSupportedException(typeof(TService).Name);
            }

            public Type ServiceAncestorMarkerType
            {
                get { return typeof(IContractMarkerTwo); }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IContractMarkerOne
        {
        }
        public interface IContractMarkerTwo
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IContractOne : IContractMarkerOne
        {
            int Value { get; }
        }
        public interface IContractTwo : IContractMarkerTwo
        {
            string Value { get; }
        }
        private class ImplOne : IContractOne
        {
            public int Value { get; set; }
        }
        private class ImplTwo : IContractTwo
        {
            public string Value { get; set; }
        }
    }
}
