using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing.Commands.Factories;
using NWheels.Testing;
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
            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
   
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

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);

            var methodOneInfo = typeof(TestTarget).GetMethod("MethodOne");
            var call = factoryUnderTest.NewMessageCallObject(methodOneInfo);

            //-- act

            var target = new TestTarget(Framework.Logger<ITestTargetLogger>());
            call.ExecuteOn(target);

            //-- assert

            Framework.TakeLog().ShouldHaveOne<ITestTargetLogger>(x => x.MethodOne());
            call.Result.ShouldBe(null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCallVoidMethodWithTwoParameters()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCallNonVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanGetCallArgumentsOfVoidMethod()
        {
            //-- arrange

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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

            var factoryUnderTest = new MethodCallObjectFactory(base.DyamicModule);
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

        public interface ITestTargetLogger : IApplicationEventLogger
        {
            [LogInfo]
            void MethodOne();
            [LogInfo]
            void MethodTwo(int num);
            [LogInfo]
            void MethodThree(int num, string str);
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
    }
}
