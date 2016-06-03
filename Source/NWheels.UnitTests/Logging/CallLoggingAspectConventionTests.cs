using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Hapil;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Hosting.Factories;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Logging.Factories;
using NWheels.Testing;

namespace NWheels.UnitTests.Logging
{
    [TestFixture]
    public class CallLoggingAspectConventionTests : NUnitEmittedTypesTestBase
    {
        private ComponentAspectFactory _factory;
        private TestThreadLogAppender _logAppender;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _factory = new ComponentAspectFactory(base.Module);
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
        public void CanLogVoidCallWithNoParameters()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = _factory.CreateInstanceOf<ITestComponent>().UsingConstructor<object, IThreadLogAppender>(realComponent, _logAppender);

            //-- Act

            decoratedComponent.ThisIsMyVoidMethod();

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "TestComponent.ThisIsMyVoidMethod", 
                "BACKEND:ThisIsMyVoidMethod",
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogVoidCallWithParameters()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = _factory.CreateInstanceOf<ITestComponent>().UsingConstructor<object, IThreadLogAppender>(realComponent, _logAppender);

            //-- Act

            decoratedComponent.ThisIsMyVoidMethodWithParameters(num: 123, str: "ABC");

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "TestComponent.ThisIsMyVoidMethodWithParameters", 
                "BACKEND:ThisIsMyVoidMethod",
            }));

            var nameValuePairs = _logAppender.GetLog()[0].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(nameValuePairs.Length, Is.EqualTo(2));
            Assert.That(nameValuePairs[0].FormatLogString(), Is.EqualTo("num=123"));
            Assert.That(nameValuePairs[1].FormatLogString(), Is.EqualTo("str=ABC"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogFunctionCallWithParameters()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = _factory.CreateInstanceOf<ITestComponent>().UsingConstructor<object, IThreadLogAppender>(realComponent, _logAppender);

            //-- Act

            var returnValue = decoratedComponent.ThisIsMyFunction(987, "XYZ");

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "TestComponent.ThisIsMyFunction", 
                "BACKEND:ThisIsMyFunction",
                "Logging call outputs", 
            }));

            var inputNameValuePairs = _logAppender.GetLog()[0].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(inputNameValuePairs.Length, Is.EqualTo(2));
            Assert.That(inputNameValuePairs[0].FormatLogString(), Is.EqualTo("num=987"));
            Assert.That(inputNameValuePairs[1].FormatLogString(), Is.EqualTo("str=XYZ"));

            var outputNameValuePairs = _logAppender.GetLog()[2].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(outputNameValuePairs.Length, Is.EqualTo(1));
            Assert.That(outputNameValuePairs[0].FormatLogString(), Is.EqualTo("return=ABC"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestComponent
        {
            void ThisIsMyVoidMethod();
            void ThisIsMyVoidMethodWithParameters(int num, string str);
            string ThisIsMyFunction(int num, string str);
            DateTime ThisIsMyMethodWithPrimitiveValues(TimeSpan time, DayOfWeek day);
            MyReplyObject ThisIsMyMethodWithLoggableObjects(MyRequestObject request);
            DayOfWeek ThisIsMyMethodWithRefOutParameters(ref int num, out string str);
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
                _logAppender.AppendLogNode(new NameValuePairLogNode("!BACKEND:ThisIsMyVoidMethod", LogLevel.Debug, LogOptions.None, exception: null));
            }
            public void ThisIsMyVoidMethodWithParameters(int num, string str)
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode("!BACKEND:ThisIsMyVoidMethod", LogLevel.Debug, LogOptions.None, exception: null));
            }
            public string ThisIsMyFunction(int num, string str)
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode("!BACKEND:ThisIsMyFunction", LogLevel.Debug, LogOptions.None, exception: null));
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
            public DayOfWeek ThisIsMyMethodWithRefOutParameters(ref int num, out string str)
            {
                num *= 2;
                str = (num + 1).ToString();
                return DayOfWeek.Tuesday;
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

        public class CompiledExample
        {
            private static readonly string _s_string_TestComponentThisIsMyFunction = "!TestComponent.ThisIsMyFunction";
            private static readonly string _s_string_CallLoggingAspectLoggingCallOutputs = "CallLoggingAspect.LoggingCallOutputs";
            
            private readonly object _target;
            private readonly IThreadLogAppender _threadLogAppender;

            public CompiledExample(object target, IThreadLogAppender threadLogAppender)
            {
                _target = target;
                _threadLogAppender = threadLogAppender;
            }

            public string ThisIsMyFunction(int num, string str)
            {
                LogNameValuePair<int> pair = new LogNameValuePair<int>
                {
                    Name = "num",
                    Value = num,
                    IsDetail = true
                };
                LogNameValuePair<string> pair2 = new LogNameValuePair<string>
                {
                    Name = "str",
                    Value = str,
                    IsDetail = true
                };
                ActivityLogNode activity = new NameValuePairActivityLogNode<int, string>(_s_string_TestComponentThisIsMyFunction, LogLevel.Debug, LogOptions.None, pair, pair2);
                this._threadLogAppender.AppendActivityNode(activity);
                try
                {
                    string str4 = ((CallLoggingAspectConventionTests.ITestComponent)this._target).ThisIsMyFunction(num, str);
                    LogNameValuePair<string> pair3 = new LogNameValuePair<string>
                    {
                        Name = CallLoggingAspectConvention.CallOutputReturnValueName,
                        Value = str4,
                        IsDetail = true
                    };
                    this._threadLogAppender.AppendLogNode(new NameValuePairLogNode<string>(_s_string_CallLoggingAspectLoggingCallOutputs, LogLevel.Debug, LogOptions.None, null, pair3));
                    return str4;
                }
                catch (Exception exception)
                {
                    ((ILogActivity)activity).Fail(exception);
                    throw;
                }
                finally
                {
                    ((IDisposable)activity).Dispose();
                }
            }
            
        }
    }
}
