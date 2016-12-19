using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Concurrency.Core;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Serialization;
using NWheels.Serialization.Factories;
using NWheels.Testing;
using NWheels.Testing.Extensions;
using NWheels.Utilities;
using Shouldly;

namespace NWheels.UnitTests.Processing.Commands.Factories
{
    [TestFixture]
    public class MethodCallObjectFactoryTests : DynamicTypeUnitTestBase
    {
        [SetUp]
        public void SetUp()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateMethodCallObject()
        {
            //-- arrange

            var methodOneInfo = typeof(TestTarget).GetMethod("MethodOne");
            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
   
            //-- act

            var call = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            //-- assert

            call.ShouldNotBe(null);
            call.MethodInfo.ShouldBeSameAs(methodOneInfo);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCallVoidMethodWithNoParameters()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());

            var methodOneInfo = typeof(TestTarget).GetMethod("MethodOne");
            var call = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            //-- act

            var target = new TestTarget(Framework.Logger<ITestTargetLogger>());
            call.ExecuteOn(target);

            //-- assert

            Framework.TakeLog().ShouldHaveOne<ITestTargetLogger>(x => x.MethodOne());
            call.Result.ShouldBe(null);
            call.ShouldHaveNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCallVoidMethodWithTwoParameters()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTarget).GetMethod("MethodThree");

            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);
            dynamic callAsDynamic = call;

            //-- act

            callAsDynamic.Num = 123;
            callAsDynamic.Str = "ABC";
            
            var target = new TestTarget(Framework.Logger<ITestTargetLogger>());
            call.ExecuteOn(target);

            //-- assert

            Framework.TakeLog().ShouldHaveOne<ITestTargetLogger>(x => x.MethodThree(123, "ABC"));
            call.Result.ShouldBe(null);
            call.ShouldHaveNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCallNonVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodTwoInfo = typeof(TestTarget).GetMethod("MethodTwo");

            var call = factoryUnderTest.NewMessageCallObject(methodTwoInfo);
            dynamic callAsDynamic = call;

            //-- act

            callAsDynamic.Num = 123;

            var target = new TestTarget(Framework.Logger<ITestTargetLogger>());
            call.ExecuteOn(target);

            //-- assert

            Framework.TakeLog().ShouldHaveOne<ITestTargetLogger>(x => x.MethodTwo(123));
            call.Result.ShouldBe("246");
            call.ShouldHaveNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanGetCallArgumentsOfVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTarget).GetMethod("MethodThree");

            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);
            dynamic callAsDynamic = call;

            callAsDynamic.Num = 123;
            callAsDynamic.Str = "ABC";

            //-- act

            var numValue = call.GetParameterValue(0);
            var strValue = call.GetParameterValue(1);

            //-- assert

            numValue.ShouldBe(123);
            strValue.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanGetCallArgumentsOfNonVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodTwoInfo = typeof(TestTarget).GetMethod("MethodTwo");

            var call = factoryUnderTest.NewMessageCallObject(methodTwoInfo);
            dynamic callAsDynamic = call;

            callAsDynamic.Num = 123;

            //-- act

            var numValue = call.GetParameterValue(0);

            //-- assert

            numValue.ShouldBe(123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanThrowOnGetCallArgumentBadIndex()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodTwoInfo = typeof(TestTarget).GetMethod("MethodTwo");
            var call = factoryUnderTest.NewMessageCallObject(methodTwoInfo);

            //-- act & assert

            Should.Throw<ArgumentOutOfRangeException>(() => {
                call.GetParameterValue(1);        
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanStoreExtensionDataProperties()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodTwoInfo = typeof(TestTarget).GetMethod("MethodTwo");
            var call = factoryUnderTest.NewMessageCallObject(methodTwoInfo);

            //-- act & assert

            call.ExtensionData.ShouldNotBeNull();
            call.ExtensionData.ShouldBeSameAs(call.ExtensionData);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPopulateFromJson()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTarget).GetMethod("MethodThree");
            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);
            var json = "{num: 123, str: 'ABC'}";

            //-- act 

            JsonConvert.PopulateObject(json, call);

            dynamic callDynamic = call;
            int numValue = callDynamic.Num;
            string strValue = callDynamic.Str;

            //-- assert

            numValue.ShouldBe(123);
            strValue.ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPopulateFromJsonAndStoreExtensionData()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTarget).GetMethod("MethodThree");
            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);
            var json = "{num: 123, str: 'ABC', value1: 'one', value2: 222}";

            //-- act 

            JsonConvert.PopulateObject(json, call);

            dynamic callDynamic = call;
            int numValue = callDynamic.Num;
            string strValue = callDynamic.Str;

            //-- assert

            numValue.ShouldBe(123);
            strValue.ShouldBe("ABC");

            call.ExtensionData["value1"].Type.ShouldBe(JTokenType.String);
            call.ExtensionData["value1"].ToString().ShouldBe("one");

            call.ExtensionData["value2"].Type.ShouldBe(JTokenType.Integer);
            call.ExtensionData["value2"].ToString().ShouldBe("222");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void VoidMethodCallObjectIsDeferred()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTarget).GetMethod("MethodOne");

            //-- act 

            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);

            //-- assert

            call.ShouldBeAssignableTo<Deferred>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NonVoidMethodCallObjectIsDeferredOfReturnType()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodTwoInfo = typeof(TestTarget).GetMethod("MethodTwo");

            //-- act 

            var call = factoryUnderTest.NewMessageCallObject(methodTwoInfo);

            //-- assert

            call.ShouldBeAssignableTo<Deferred<string>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void VoidPromiseMethodCallObjectIsDeferred()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestPromiseTarget).GetMethod("MethodOne");

            //-- act 

            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);

            //-- assert

            call.ShouldBeAssignableTo<Deferred>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NonVoidPromiseMethodCallObjectIsDeferred()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestPromiseTarget).GetMethod("MethodThree");

            //-- act 

            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);

            //-- assert

            call.ShouldBeAssignableTo<Deferred<DateTime>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void VoidTaskMethodCallObjectIsTaskBasedDeferred()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodOneInfo = typeof(TestTaskTarget).GetMethod("MethodOne");

            //-- act 

            var call = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            //-- assert

            call.ShouldBeAssignableTo<TaskBasedDeferred>();
            ((TaskBasedDeferred)call).Task.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NonVoidTaskMethodCallObjectIsTaskBasedDeferred()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTaskTarget).GetMethod("MethodThree");

            //-- act 

            var call = factoryUnderTest.NewMessageCallObject(methodThreeInfo);

            //-- assert

            call.ShouldBeAssignableTo<TaskBasedDeferred<DateTime>>();
            ((TaskBasedDeferred<DateTime>)call).Task.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeInputOfMethodWithParameters()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTarget).GetMethod("MethodThree");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodThreeInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodThreeInfo);

            senderCall.SetParameterValue(0, 123);
            senderCall.SetParameterValue(1, "ABC");

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            senderCall.Serializer.SerializeInput(serializationContext);

            wire.Position = 0;

            receiverCall.Serializer.DeserializeInput(deserializationContext);
            
            //-- assert

            receiverCall.GetParameterValue(0).ShouldBe(123);
            receiverCall.GetParameterValue(1).ShouldBe("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeInputOMethodWithNoParameters()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodOneInfo = typeof(TestTarget).GetMethod("MethodOne");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodOneInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            senderCall.Serializer.SerializeInput(serializationContext);

            wire.Position = 0;

            receiverCall.Serializer.DeserializeInput(deserializationContext);

            //-- assert

            // it didn't fail
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeOutputOfNonVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodTwoInfo = typeof(TestTarget).GetMethod("MethodTwo");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodTwoInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodTwoInfo);

            receiverCall.SetParameterValue(0, 123);
            receiverCall.ExecuteOn(new TestTarget(Framework.Logger<ITestTargetLogger>())); // should get "246" in result

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            receiverCall.Serializer.SerializeOutput(serializationContext);

            wire.Position = 0;

            senderCall.Serializer.DeserializeOutput(deserializationContext);

            //-- assert

            senderCall.Result.ShouldBe("246");
            receiverCall.ShouldHaveNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeOutputOfVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodOneInfo = typeof(TestTarget).GetMethod("MethodOne");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodOneInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            receiverCall.ExecuteOn(new TestTarget(Framework.Logger<ITestTargetLogger>())); // should get nothing in result

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            receiverCall.Serializer.SerializeOutput(serializationContext);

            wire.Position = 0;

            senderCall.Serializer.DeserializeOutput(deserializationContext);

            //-- assert

            receiverCall.ShouldHaveNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeOutputReturnOfTypeTaskNonGeneric()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodOneInfo = typeof(TestTaskTarget).GetMethod("MethodOne");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodOneInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            receiverCall.ExecuteOn(new TestTaskTarget()); // should get nothing in result

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            receiverCall.Serializer.SerializeOutput(serializationContext);

            wire.Position = 0;

            senderCall.Serializer.DeserializeOutput(deserializationContext);

            //-- assert

            receiverCall.ShouldHaveNoErrors();

            wire.Length.ShouldBe(0);
            senderCall.Result.ShouldBeAssignableTo<Task>();
            ((Task)senderCall.Result).IsCompleted.ShouldBe(true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeOutputReturnOfTypeTaskOfT()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodThreeInfo = typeof(TestTaskTarget).GetMethod("MethodThree");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodThreeInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodThreeInfo);

            receiverCall.ExecuteOn(new TestTaskTarget()); // should get a completed Task<DateTime> in result

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            receiverCall.Serializer.SerializeOutput(serializationContext);

            wire.Position = 0;

            senderCall.Serializer.DeserializeOutput(deserializationContext);

            //-- assert

            receiverCall.ShouldHaveNoErrors();

            wire.Length.ShouldBeGreaterThan(0);
            senderCall.Result.ShouldBeOfType<Task<DateTime>>();
            ((Task<DateTime>)senderCall.Result).IsCompleted.ShouldBe(true);
            ((Task<DateTime>)senderCall.Result).Result.ShouldBe(new DateTime(2012, 11, 10));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeOutputReturnOfTypeTaskOfNonPrimitiveStruct()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodFiveInfo = typeof(TestTaskTarget).GetMethod("MethodFive");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodFiveInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodFiveInfo);

            receiverCall.SetParameterValue(0, 123);
            receiverCall.SetParameterValue(1, "ABC");
            receiverCall.SetParameterValue(2, new DtoFour {
                Str = "DEF",
                Num = 456,
                Date = new DateTime(2013, 12, 11)
            });
            receiverCall.ExecuteOn(new TestTaskTarget()); // should get a completed Task<DtoFive>

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext);

            //-- act 

            receiverCall.Serializer.SerializeOutput(serializationContext);

            wire.Position = 0;

            senderCall.Serializer.DeserializeOutput(deserializationContext);

            //-- assert

            receiverCall.ShouldHaveNoErrors();

            wire.Length.ShouldBeGreaterThan(0);
            senderCall.Result.ShouldBeOfType<Task<DtoFive>>();
            ((Task<DtoFive>)senderCall.Result).IsCompleted.ShouldBe(true);

            var result = ((Task<DtoFive>)senderCall.Result).Result;

            result.Num.ShouldBe(579);
            result.Str.ShouldBe("ABCDEF");
            result.Date.ShouldBe(new DateTime(2014, 12, 11));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSerializeOutputReturnOfTypeTaskOfInterface()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(DyamicModule, Resolve<CompactSerializerFactory>());
            var methodSixInfo = typeof(TestTaskTarget).GetMethod("MethodSix");
            var senderCall = factoryUnderTest.NewMessageCallObject(methodSixInfo);
            var receiverCall = factoryUnderTest.NewMessageCallObject(methodSixInfo);

            receiverCall.SetParameterValue(0, 123);
            receiverCall.SetParameterValue(1, "ABC");
            receiverCall.SetParameterValue(2, new DtoFour {
                Num = 1000,
                Str = "ZZZZ",
                Date = new DateTime(2020, 1, 1)
            });

            receiverCall.ExecuteOn(new TestTaskTarget()); // should get a completed Task<IDtoSix> in result

            CompactSerializationContext serializationContext;
            CompactDeserializationContext deserializationContext;
            var wire = CreateSerializationWire(out serializationContext, out deserializationContext, typeof(DtoSixA), typeof(DtoSixB));

            //-- act 

            receiverCall.Serializer.SerializeOutput(serializationContext);

            wire.Position = 0;

            senderCall.Serializer.DeserializeOutput(deserializationContext);

            //-- assert

            receiverCall.ShouldHaveNoErrors();

            wire.Length.ShouldBeGreaterThan(0);
            senderCall.Result.ShouldBeOfType<Task<IDtoSix>>();
            ((Task<IDtoSix>)senderCall.Result).IsCompleted.ShouldBe(true);

            var result = ((Task<IDtoSix>)senderCall.Result).Result;

            result.ShouldBeOfType<DtoSixB>();
            result.Num.ShouldBe(1123);
            result.Str.ShouldBe("ABCZZZZ");
            result.Date.ShouldBe(new DateTime(2021, 1, 1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemoryStream CreateSerializationWire(
            out CompactSerializationContext serializationContext, 
            out CompactDeserializationContext deserializationContext,
            params Type[] knownTypes)
        {
            var serializer = Framework.Components.Resolve<CompactSerializer>();
            var serializerDictionary = new StaticCompactSerializerDictionary();

            if (knownTypes != null)
            {
                foreach (var type in knownTypes)
                {
                    serializerDictionary.RegisterType(type);
                }
            }

            var wire = new MemoryStream();
            var writer = new CompactBinaryWriter(wire);
            var reader = new CompactBinaryReader(wire);
            
            serializationContext = new CompactSerializationContext(serializer, serializerDictionary, writer);
            deserializationContext = new CompactDeserializationContext(serializer, serializerDictionary, reader, Framework.Components);
            
            return wire;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestTargetLogger : IApplicationEventLogger
        {
            [LogInfo]
            void MethodOne();
            [LogInfo]
            void MethodTwo(int num);
            [LogInfo]
            void MethodThree(int num, string str);
            [LogInfo]
            void MethodFour(int num, string str);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestTarget
        {
            private readonly ITestTargetLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestTarget(ITestTargetLogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void MethodOne()
            {
                _logger.MethodOne();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string MethodTwo(int num)
            {
                _logger.MethodTwo(num);
                return (num * 2).ToString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void MethodThree(int num, string str)
            {
                _logger.MethodThree(num, str);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestPromiseTarget
        {
            public Promise MethodOne()
            {
                return Promise.Resolved();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise<string> MethodTwo(int num)
            {
                return (num * 2).ToString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise<DateTime> MethodThree(int num, string str)
            {
                return new DateTime(2012, 11, 10, 0, 0, 0);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise<DtoFour> MethodFour(DtoFive five, IDtoSix six)
            {
                return new DtoFour() {
                    Num = 1000 + five.Num,
                    Str = five.Str + five.Str,
                    Date = six.Date.AddYears(1)
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise<DtoFive> MethodFive(int num, string str, DtoFour four)
            {
                return new DtoFive() {
                    Num = num + four.Num,
                    Str = str + four.Str,
                    Date = four.Date.AddYears(1)
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise<IDtoSix> MethodSix(int num, string str, DtoFour four)
            {
                return new DtoSixB() {
                    Num = num + four.Num,
                    Str = str + four.Str,
                    Date = four.Date.AddYears(1)
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestTaskTarget
        {
            public Task MethodOne()
            {
                return Task.FromResult(true);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<string> MethodTwo(int num)
            {
                return Task.FromResult((num * 2).ToString());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<DateTime> MethodThree(int num, string str)
            {
                return Task.FromResult(new DateTime(2012, 11, 10, 0, 0, 0));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<DtoFour> MethodFour(DtoFive five, IDtoSix six)
            {
                return Task.FromResult(new DtoFour() {
                    Num = 1000 + five.Num,
                    Str = five.Str + five.Str,
                    Date = six.Date.AddYears(1)
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<DtoFive> MethodFive(int num, string str, DtoFour four)
            {
                return Task.FromResult<DtoFive>(new DtoFive() {
                    Num = num + four.Num,
                    Str = str + four.Str,
                    Date = four.Date.AddYears(1)
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<IDtoSix> MethodSix(int num, string str, DtoFour four)
            {
                return Task.FromResult<IDtoSix>(new DtoSixB() {
                    Num = num + four.Num,
                    Str = str + four.Str,
                    Date = four.Date.AddYears(1)
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DtoFour
        {
            public int Num { get; set; }
            public string Str { get; set; }
            public DateTime Date { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct DtoFive
        {
            public int Num { get; set; }
            public string Str { get; set; }
            public DateTime Date { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IDtoSix
        {
            int Num { get; set; }
            string Str { get; set; }
            DateTime Date { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DtoSixA : IDtoSix
        {
            public int Num { get; set; }
            public string Str { get; set; }
            public DateTime Date { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DtoSixB : IDtoSix
        {
            public int Num { get; set; }
            public string Str { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
