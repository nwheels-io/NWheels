using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hapil;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Core.Logging;
using NWheels.Logging;
using NWheels.Testing;

namespace NWheels.Core.UnitTests.Logging
{
    [TestFixture]
    public class CallLoggingAspectFactoryTest : NUnitEmittedTypesTestBase
    {
        private ConventionObjectFactory _factory;
        private TestThreadLogAppender _logAppender;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _factory = new CallLoggingAspectFactory(base.Module);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _logAppender = new TestThreadLogAppender(new TestFramework(base.Module));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateAspectObject()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);

            //-- Act
            
            var decoratedComponent = _factory.CreateInstanceOf<ITestComponent>().UsingConstructor<object, IThreadLogAppender>(realComponent, _logAppender);

            //-- Assert

            Assert.That(decoratedComponent, Is.Not.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogMethodCall()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = _factory.CreateInstanceOf<ITestComponent>().UsingConstructor<object, IThreadLogAppender>(realComponent, _logAppender);

            //-- Act

            decoratedComponent.ThisIsMyVoidMethod();
            var returnValue = decoratedComponent.ThisIsMyFunction(123, "XYZ");

            //-- Assert

            Assert.That(returnValue, Is.EqualTo("ABC"));
            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "TestComponent::ThisIsMyVoidMethod", 
                "BACKEND::ThisIsMyVoidMethod",
                "TestComponent::ThisIsMyFunction", 
                "BACKEND::ThisIsMyFunction"
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestComponent
        {
            void ThisIsMyVoidMethod();
            string ThisIsMyFunction(int num, string str);
            DateTime ThisIsMyMethodWithPrimitiveValues(TimeSpan time, DayOfWeek day);
            MyReplyObject ThisIsMyMethodWithLoggableObjects(MyRequestObject request);
            XElement ThisIsMyMethodWithXmlSerializableObjects(XElement input);
            object ThisIsMyMethodWithNonLoggableObjects(Stream data);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RealComponent : ITestComponent
        {
            private readonly IThreadLogAppender _logAppender;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public RealComponent(IThreadLogAppender logAppender)
            {
                _logAppender = logAppender;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ThisIsMyVoidMethod()
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode("BACKEND::ThisIsMyVoidMethod", LogLevel.Debug, exception: null));
            }
            public string ThisIsMyFunction(int num, string str)
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode("BACKEND::ThisIsMyFunction", LogLevel.Debug, exception: null));
                return "ABC";
            }
            public DateTime ThisIsMyMethodWithPrimitiveValues(TimeSpan time, DayOfWeek day)
            {
                return new DateTime(2010, 10, 10);
            }
            public MyReplyObject ThisIsMyMethodWithLoggableObjects(MyRequestObject request)
            {
                return new MyReplyObject();
            }
            public XElement ThisIsMyMethodWithXmlSerializableObjects(XElement input)
            {
                return new XElement("ABC");
            }
            public object ThisIsMyMethodWithNonLoggableObjects(Stream data)
            {
                return new SomeObject();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MyRequestObject
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MyReplyObject
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SomeObject
        {
        }
    }
}
