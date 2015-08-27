using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        public void CanCallMethodWithNoParameters()
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCallMethodWithTwoParameters()
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

            public void MethodTwo(int num)
            {
                _logger.MethodTwo(num);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void MethodThree(int num, string str)
            {
                _logger.MethodThree(num, str);
            }
        }
    }
}
