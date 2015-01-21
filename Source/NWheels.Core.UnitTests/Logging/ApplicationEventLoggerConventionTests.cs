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

        [Test]
        public void CanAppendEmptyMessage()
        {
            //-- Arrange

            var logger = CreateTestLogger();

            //-- Act

            logger.ThisIsMyEmptyDebugMessage();

            //-- Assert

            var log = _log.TakeLog();

            Assert.That(log.Length, Is.EqualTo(1));
            Assert.That(log[0].Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(log[0].SingleLineText, Is.EqualTo("This is my empty debug message"));
            Assert.That(log[0].FullDetailsText, Is.Null);
            Assert.That(log[0].Exception, Is.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAppendMessageWithParameters()
        {
            //-- Arrange

            var logger = CreateTestLogger();

            //-- Act

            logger.ThisIsMyDebugMessage(num: 123, str: "ABC");

            //-- Assert

            var log = _log.TakeLog();

            Assert.That(log.Length, Is.EqualTo(1));
            Assert.That(log[0].Level, Is.EqualTo(LogLevel.Debug));
            Assert.That(log[0].SingleLineText, Is.EqualTo("This is my debug message: num=123, str=ABC"));
            Assert.That(log[0].FullDetailsText, Is.Null);
            Assert.That(log[0].Exception, Is.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAppendMessageWithException()
        {
            //-- Arrange

            var logger = CreateTestLogger();
            var exception = new DivideByZeroException();

            //-- Act

            logger.ThisIsMyErrorMessageWithExceptionParameter(num: 123, str: "ABC", e: exception);

            //-- Assert

            var log = _log.TakeLog();

            Assert.That(log.Length, Is.EqualTo(1));
            Assert.That(log[0].Level, Is.EqualTo(LogLevel.Error));
            Assert.That(log[0].SingleLineText, Is.EqualTo("This is my error message with exception parameter: num=123, str=ABC"));
            Assert.That(log[0].FullDetailsText, Is.EqualTo(exception.ToString()));
            Assert.That(log[0].Exception, Is.SameAs(exception));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAppendErrorMessageAndThrow()
        {
            //-- Arrange

            var logger = CreateTestLogger();

            //-- Act

            try
            {
                throw logger.ThisIsMyErrorMessageThatCreatesException();
            }
            catch ( TestErrorException e )
            {
                //-- Assert

                var log = _log.TakeLog();

                Assert.That(e.Message, Is.EqualTo("This is my error message that creates exception"));
                Assert.That(log.Length, Is.EqualTo(1));
                Assert.That(log[0].Level, Is.EqualTo(LogLevel.Error));
                Assert.That(log[0].SingleLineText, Is.EqualTo("This is my error message that creates exception"));
                Assert.That(e.ToString().StartsWith(log[0].FullDetailsText));
                Assert.That(log[0].Exception, Is.SameAs(e));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RegularLogsAndActivitiesDoNotStartThreadLog()
        {
            //-- Arrange

            var logger = CreateTestLogger();

            //-- Act

            logger.ThisIsMyCriticalMessage();
            logger.ThisIsMyActivity();

            //-- Assert

            Assert.IsFalse(_log.StartedThreadTaskType.HasValue);
            Assert.IsFalse(_log.StartedThreadLogIndex.HasValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ActivityMarkedAsLogThreadWillStartThreadLog()
        {
            //-- Arrange

            var logger = CreateTestLogger();

            //-- Act

            logger.ThisIsMyCriticalMessage();
            logger.ThisIsMyThread(123, "ABC");
            logger.ThisIsMyActivity();

            //-- Assert

            Assert.That(_log.StartedThreadTaskType, Is.EqualTo(ThreadTaskType.ScheduledJob));
            Assert.That(_log.StartedThreadLogIndex, Is.EqualTo(1));
            
            Assert.That(_log.GetLog()[1], Is.InstanceOf<ActivityLogNode>());
            Assert.That(_log.GetLog()[1].SingleLineText, Is.EqualTo("This is my thread: num=123, str=ABC"));
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
            TestErrorException ThisIsMyErrorMessageThatCreatesException();
            
            [LogCritical]
            void ThisIsMyCriticalMessage();
            
            [LogCritical]
            TestErrorException ThisIsMyCriticalMessageThatCreatesException();
            
            [LogActivity]
            ILogActivity ThisIsMyActivity();
            
            [LogActivity]
            ILogActivity ThisIsMyActivityWithParameters(int num, string str);
            
            [LogThread(ThreadTaskType.ScheduledJob)]
            ILogActivity ThisIsMyThread(int num, string str);
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
