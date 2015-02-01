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
            Assert.That(log[0].FullDetailsText, Is.EqualTo(exception.ToString() + Environment.NewLine));
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

        [Test]
        public void CanUseParametersOfNullableValueTypes()
        {
            //-- Arrange

            var logger = CreateTestLogger();
            var date1 = new DateTime(2015, 1, 1);
            var date2 = new DateTimeOffset(new DateTime(2015, 2, 2), TimeSpan.FromHours(-7));

            //-- Act

            logger.AnActivityWithNullableParameterTypes(date1, date2);
            logger.AnActivityWithNullableParameterTypes(null, null);

            //-- Assert

            Assert.That(_log.GetLogStrings(), Is.EqualTo(new[] {
                string.Format("An activity with nullable parameter types: date1={0}, date2={1}", date1, date2),
                "An activity with nullable parameter types: date1=, date2="
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSpecifyParameterFormat()
        {
            //-- Arrange

            var logger = CreateTestLogger();
            var date1 = new DateTime(2015, 1, 1);
            var date2 = new DateTimeOffset(new DateTime(2015, 2, 2), TimeSpan.FromHours(-7));

            //-- Act

            logger.LogMessageWithFormattedParameters(date1, date2);

            //-- Assert

            Assert.That(_log.GetLogStrings(), Is.EqualTo(new[] {
                "Log message with formatted parameters: date1=2015-01-01, date2=Feb 02 2015",
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanMoveParametersIntoFullDetailsText()
        {
            //-- Arrange

            var logger = CreateTestLogger();
            var dateTime = new DateTime(2015, 1, 1);
            var dateTimeOffset = new DateTimeOffset(new DateTime(2015, 2, 2), TimeSpan.FromHours(-7));

            //-- Act

            logger.LogMessageWithFormattedAndDetailParameters(123, "ABC", dateTime, dateTimeOffset);

            //-- Assert

            Assert.That(_log.GetLog()[0].SingleLineText, Is.EqualTo(
                "Log message with formatted and detail parameters: num=123, dateTime=2015-01-01"
            ));
            Assert.That(_log.GetLog()[0].FullDetailsText, Is.EqualTo(
                "Str=ABC" + Environment.NewLine + "DateTimeOffset=Feb 02 2015" + Environment.NewLine
            ));
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

            [LogActivity]
            ILogActivity AnActivityWithNullableParameterTypes(DateTime? date1, DateTimeOffset? date2);

            [LogDebug]
            void LogMessageWithFormattedParameters(
                [Format("yyyy-MM-dd")] DateTime? date1, 
                [Format("MMM dd yyyy")] DateTimeOffset? date2);

            [LogVerbose]
            void LogMessageWithFormattedAndDetailParameters(
                int num,
                [Detail] string str,
                [Format("yyyy-MM-dd")] DateTime dateTime,
                [Format("MMM dd yyyy"), Detail] DateTimeOffset? dateTimeOffset);
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
