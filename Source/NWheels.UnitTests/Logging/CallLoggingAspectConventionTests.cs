using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autofac;
using Hapil;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Hosting.Core;
using NWheels.Hosting.Factories;
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Logging.Factories;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Logging
{
    [TestFixture]
    public class CallLoggingAspectConventionTests : DynamicTypeUnitTestBase
    {
        private ComponentAspectFactory _factory;
        private TestThreadLogAppender _logAppender;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _logAppender = new TestThreadLogAppender(new TestFramework(base.DyamicModule));
            Framework.UpdateComponents(b => b.RegisterInstance(_logAppender).As<IThreadLogAppender>());

            var aspectPipeline = new Pipeline<IComponentAspectProvider>(new IComponentAspectProvider[] {
                new CallLoggingAspectConvention.AspectProvider(),
            });

            _factory = new ComponentAspectFactory(Framework.Components, base.DyamicModule, aspectPipeline);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateAspectObject()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);

            //-- Act
            
            var decoratedComponent = _factory.CreateProxy(realComponent);

            //-- Assert

            Assert.That(decoratedComponent, Is.Not.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogVoidCallWithNoParameters()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            decoratedComponent.ThisIsMyVoidMethod();

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyVoidMethod", 
                "BACKEND:ThisIsMyVoidMethod",
            }));

            var activity = (ActivityLogNode)_logAppender.GetLog()[0];

            activity.IsSuccess.ShouldBeTrue();
            (activity.Level <= LogLevel.Info).ShouldBeTrue();
            activity.Exception.ShouldBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogVoidCallWithParameters()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            decoratedComponent.ThisIsMyVoidMethodWithParameters(num: 123, str: "ABC");

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyVoidMethodWithParameters", 
                "BACKEND:ThisIsMyVoidMethodWithParameters(num=123,str=ABC)",
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
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            var returnValue = decoratedComponent.ThisIsMyFunction(987, "XYZ");

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyFunction", 
                "BACKEND:ThisIsMyFunction(num=987,str=XYZ)",
                "Logging call outputs", 
            }));

            Assert.That(returnValue, Is.EqualTo("ABC"));

            var inputNameValuePairs = _logAppender.GetLog()[0].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(inputNameValuePairs.Length, Is.EqualTo(2));
            Assert.That(inputNameValuePairs[0].FormatLogString(), Is.EqualTo("num=987"));
            Assert.That(inputNameValuePairs[1].FormatLogString(), Is.EqualTo("str=XYZ"));

            var outputNameValuePairs = _logAppender.GetLog()[2].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(outputNameValuePairs.Length, Is.EqualTo(1));
            Assert.That(outputNameValuePairs[0].FormatLogString(), Is.EqualTo("return=ABC"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogMethodCallWithRefOutParameters()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            var num = 123;
            string str;
            var returnValue = decoratedComponent.ThisIsMyMethodWithRefOutParameters(ref num, out str);

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyMethodWithRefOutParameters", 
                "BACKEND:ThisIsMyMethodWithRefOutParameters(num=123)",
                "Logging call outputs", 
            }));

            Assert.That(num, Is.EqualTo(246));
            Assert.That(str, Is.EqualTo("247"));
            Assert.That(returnValue, Is.EqualTo(DayOfWeek.Tuesday));

            var inputNameValuePairs = _logAppender.GetLog()[0].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(inputNameValuePairs.Length, Is.EqualTo(1));
            Assert.That(inputNameValuePairs[0].FormatLogString(), Is.EqualTo("num=123"));

            var outputNameValuePairs = _logAppender.GetLog()[2].NameValuePairs.Where(nvp => !nvp.IsBaseValue()).ToArray();

            Assert.That(outputNameValuePairs.Length, Is.EqualTo(3));
            Assert.That(outputNameValuePairs[0].FormatLogString(), Is.EqualTo("num=246"));
            Assert.That(outputNameValuePairs[1].FormatLogString(), Is.EqualTo("str=247"));
            Assert.That(outputNameValuePairs[2].FormatLogString(), Is.EqualTo("return=Tuesday"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogMethodCallWithExplicitInterfaceImplementation()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = _factory.CreateProxy(realComponent);

            //-- Act

            ((IAnotherComponent)decoratedComponent).ThisIsMyVoidMethod();
            ((IAnotherComponent)decoratedComponent).ThisIsAnotherVoidMethod();

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.IAnotherComponent.ThisIsMyVoidMethod", 
                "BACKEND:IAnotherComponent::ThisIsMyVoidMethod",
                "RealComponent.ThisIsAnotherVoidMethod", 
                "BACKEND:ThisIsAnotherVoidMethod",
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CallsToPropertyAccessorsAreNotLogged()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            var propertyValue = decoratedComponent.ThisIsMyProperty;
            decoratedComponent.ThisIsMyProperty = 222;

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "BACKEND:ThisIsMyProperty.GET",
                "BACKEND:ThisIsMyProperty.SET(value=222)",
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogMethodCallThroughDelegate()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            System.Action actionDelegate = decoratedComponent.ThisIsMyVoidMethod;

            //-- Act

            actionDelegate();

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyVoidMethod", 
                "BACKEND:ThisIsMyVoidMethod",
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogExceptionOnCallToVoidMethod()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            Should.Throw<TestErrorException>(
                () => {
                    decoratedComponent.ThisIsMyVoidMethodWithParameters(num: -1, str: "ZZZ");
                });

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyVoidMethodWithParameters", 
                "BACKEND:ThisIsMyVoidMethodWithParameters(num=-1,str=ZZZ)",
            }));

            var activity = (ActivityLogNode)_logAppender.GetLog()[0];

            activity.IsSuccess.ShouldBeFalse();
            activity.Level.ShouldBe(LogLevel.Error);
            activity.Exception.ShouldBeOfType<TestErrorException>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLogExceptionOnCallToFunction()
        {
            //-- Arrange

            var realComponent = new RealComponent(_logAppender);
            var decoratedComponent = (ITestComponent)_factory.CreateProxy(realComponent);

            //-- Act

            Should.Throw<TestErrorException>(
                () => {
                    decoratedComponent.ThisIsMyFunction(num: -1, str: "ZZZ");
                });

            //-- Assert

            Assert.That(_logAppender.GetLogStrings(), Is.EqualTo(new[] {
                "RealComponent.ThisIsMyFunction", 
                "BACKEND:ThisIsMyFunction(num=-1,str=ZZZ)",
            }));

            var activity = (ActivityLogNode)_logAppender.GetLog()[0];

            activity.IsSuccess.ShouldBeFalse();
            activity.Level.ShouldBe(LogLevel.Error);
            activity.Exception.ShouldBeOfType<TestErrorException>();
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
            int ThisIsMyProperty { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAnotherComponent
        {
            void ThisIsMyVoidMethod();
            void ThisIsAnotherVoidMethod();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RealComponent : ITestComponent, IAnotherComponent
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
                _logAppender.AppendLogNode(new NameValuePairLogNode(
                    "!BACKEND:ThisIsMyVoidMethod", 
                    LogLevel.Debug, LogOptions.None, exception: null));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void ThisIsMyVoidMethodWithParameters(int num, string str)
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode(
                    string.Format("!BACKEND:ThisIsMyVoidMethodWithParameters(num={0},str={1})", num, str), 
                    LogLevel.Debug, LogOptions.None, exception: null));

                if (num < 0)
                {
                    throw new TestErrorException();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public string ThisIsMyFunction(int num, string str)
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode(
                    string.Format("!BACKEND:ThisIsMyFunction(num={0},str={1})", num, str),
                    LogLevel.Debug, LogOptions.None, exception: null));

                if (num < 0)
                {
                    throw new TestErrorException();
                }
                
                return "ABC";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public DateTime ThisIsMyMethodWithPrimitiveValues(TimeSpan time, DayOfWeek day)
            {
                return new DateTime(2010, 10, 10);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public MyReplyObject ThisIsMyMethodWithLoggableObjects(MyRequestObject request)
            {
                return new MyReplyObject();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public DayOfWeek ThisIsMyMethodWithRefOutParameters(ref int num, out string str)
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode(
                    string.Format("!BACKEND:ThisIsMyMethodWithRefOutParameters(num={0})", num), 
                    LogLevel.Debug, LogOptions.None, exception: null));

                num *= 2;
                str = (num + 1).ToString();
                return DayOfWeek.Tuesday;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public XElement ThisIsMyMethodWithXmlSerializableObjects(XElement input)
            {
                return new XElement("ABC");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public object ThisIsMyMethodWithNonLoggableObjects(Stream data)
            {
                return new SomeObject();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int ThisIsMyProperty
            {
                get
                {
                    _logAppender.AppendLogNode(new NameValuePairLogNode(
                        "!BACKEND:ThisIsMyProperty.GET",
                        LogLevel.Debug, LogOptions.None, exception: null));
                    return 111;
                }
                set
                {
                    _logAppender.AppendLogNode(new NameValuePairLogNode(
                        string.Format("!BACKEND:ThisIsMyProperty.SET(value={0})", value),
                        LogLevel.Debug, LogOptions.None, exception: null));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            void IAnotherComponent.ThisIsMyVoidMethod()
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode(
                    "!BACKEND:IAnotherComponent::ThisIsMyVoidMethod", 
                    LogLevel.Debug, LogOptions.None, exception: null));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            void IAnotherComponent.ThisIsAnotherVoidMethod()
            {
                _logAppender.AppendLogNode(new NameValuePairLogNode(
                    "!BACKEND:ThisIsAnotherVoidMethod", 
                    LogLevel.Debug, LogOptions.None, exception: null));
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestErrorException : Exception
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
