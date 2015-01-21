//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Autofac;
//using NUnit.Framework;

//namespace NWheels.Core.UnitTests
//{
//    [TestFixture]
//    public class AutoTests
//    {
//        [Test]
//        public void CanRegisterAutoFactoryForAuto()
//        {
//            //-- Arrange

//            var builder = new ContainerBuilder();
            
//            builder.RegisterGeneric(typeof(AutoFactory<>)).As(typeof(Auto<>)).SingleInstance();
//            builder.RegisterType<AConsumerComponent>();
            
//            var container = builder.Build();

//            //-- Act 

//            var one = container.Resolve<Auto<IContractOne>>().Instance;
//            var two = container.Resolve<Auto<IContractTwo>>().Instance;
//            var consumer = container.Resolve<AConsumerComponent>();

//            //-- Assert

//            Assert.That(one.Value, Is.EqualTo(123));
//            Assert.That(two.Value, Is.EqualTo("ABC"));
//            Assert.That(consumer.ValueOne, Is.EqualTo(123));
//            Assert.That(consumer.ValueTwo, Is.EqualTo("ABC"));
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public class AConsumerComponent
//        {
//            public AConsumerComponent(Auto<IContractOne> one, Auto<IContractTwo> two)
//            {
//                ValueOne = one.Instance.Value;
//                ValueTwo = two.Instance.Value;
//            }

//            //-------------------------------------------------------------------------------------------------------------------------------------------------

//            public int ValueOne { get; set; }
//            public string ValueTwo { get; set; }
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        private class AutoFactory<T> : Auto<T>
//            where T : class
//        {
//            public AutoFactory()
//                : base(CreateInstance())
//            {
//            }

//            //-------------------------------------------------------------------------------------------------------------------------------------------------

//            private static T CreateInstance()
//            {
//                if ( typeof(T) == typeof(IContractOne) )
//                {
//                    return (T)(object)(new ImplOne {
//                        Value = 123
//                    });
//                }
//                if ( typeof(T) == typeof(IContractTwo) )
//                {
//                    return (T)(object)(new ImplTwo {
//                        Value = "ABC"
//                    });
//                }

//                throw new NotSupportedException(typeof(T).Name);
//            }
//        }

//        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public interface IContractOne
//        {
//            int Value { get; }
//        }
//        public interface IContractTwo
//        {
//            string Value { get; }
//        }
//        private class ImplOne : IContractOne
//        {
//            public int Value { get; set; }
//        }
//        private class ImplTwo : IContractTwo
//        {
//            public string Value { get; set; }
//        }
//    }
//}
