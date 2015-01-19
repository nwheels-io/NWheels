using Hapil;
using Hapil.Testing.NUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Core.Logging;
using NWheels.Logging;
using NWheels.Testing;

namespace NWheels.Core.UnitTests.Logging
{
    [TestFixture]
    public class ApplicationEventLoggerConventionTests : NUnitEmittedTypesTestBase
    {
        private ConventionObjectFactory _factory;
        private TestThreadLogAppender _log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _factory = new ConventionObjectFactory(
                base.Module, 
                new ApplicationEventLoggerConvention());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _log = new TestThreadLogAppender();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanEmitLogger()
        {
            //-- Act

            var logger = CreateTestLogger();

            //-- Assert

            Assert.That(logger, Is.Not.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ITestLogger CreateTestLogger()
        {
            return _factory.CreateInstanceOf<ITestLogger>().UsingConstructor<IThreadLogAppender>(_log);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestLogger : IApplicationEventLogger
        {
            [LogDebug]
            void ThisIsMyEmptyDebugMessage();
            [LogDebug]
            void ThisIsMyDebugMessage(int num, string str);
            [LogVerbose]
            void ThisIsMyVerboseMessageWithCollection(DayOfWeek day, IEnumerable<DateTime> dates);
            [LogInfo]
            void ThisIsMyInfoMessageWithDefaultParameters(string first, string second = null, int third = 12345);
            [LogError]
            void ThisIsMyEmptyErrorMessage();
            [LogError]
            void ThisIsMyErrorMessageWithExceptionParameter(int num, string str, Exception e);
            [LogError]
            TestErrorException ThisIsMyErrorMessagesThatCreatesException();
            [LogCritical]
            void ThisIsMyCriticalMessage();
            [LogCritical]
            TestErrorException ThisIsMyCriticalMessageThatCreatesException();
            [LogActivity]
            ILogActivity ThisIsMyActivity();
            [LogActivity]
            ILogActivity ThisIsMyActivityWithParameters(int num, string str);
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestErrorException : Exception
        {
            public TestErrorException(string message)
                : base(message)
            {
            }
        }
    }
}
