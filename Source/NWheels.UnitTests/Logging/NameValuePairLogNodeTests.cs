using System;
using NUnit.Framework;
using NWheels.Logging;
using NWheels.Logging.Core;

namespace NWheels.UnitTests.Logging
{
    [TestFixture]
    public class NameValuePairLogNodeTests : ThreadLogUnitTestBase
    {
        private const string TestLogId = 
            "e99f7886838e4c37b433888132ef5f86";

        private const string ExpectedBaseNameValuePairs = 
            "2015-01-30 15:22:54.345 app=A1 node=N1 instance=I1 env=E1 message=Test.MessageOne level=Info logid=e99f7886838e4c37b433888132ef5f86";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLog _threadLog;
        private FormattedActivityLogNode _rootActivity;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            Framework.NodeConfiguration.ApplicationName = "A1";
            Framework.NodeConfiguration.NodeName = "N1";
            Framework.NodeConfiguration.InstanceId = "I1";
            Framework.NodeConfiguration.EnvironmentName = "E1";
            Framework.PresetUtcNow = new DateTime(2015, 1, 30, 15, 22, 54, 345);

            _rootActivity = new FormattedActivityLogNode("root");

            Framework.PresetGuids.Enqueue(new Guid(TestLogId));
            _threadLog = new ThreadLog(Framework, Clock, Registry, Anchor, ThreadTaskType.Unspecified, _rootActivity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ZeroValuesNoException()
        {
            //-- Arrange

            var node = new NameValuePairLogNode("Test.MessageOne", LogLevel.Info, exception: null);
            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one"));
            Assert.That(nameValuePairs, Is.EqualTo(ExpectedBaseNameValuePairs));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ZeroValuesWithException()
        {
            //-- Arrange

            var exception = new DivideByZeroException();
            var node = new NameValuePairLogNode("Test.MessageOne", LogLevel.Info, exception);
            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var fullDetailsText = node.FullDetailsText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one"));
            Assert.That(fullDetailsText.Contains(exception.ToString()));
            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs + " exceptionType=System.DivideByZeroException exception=\"Attempted to divide by zero.\""
            ));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void OneValueNoFormatNoDetailNoException()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string>("Test.MessageOne", LogLevel.Info, exception: null, value1: new LogNameValuePair<string> {
                Name = "accountId",
                Value = "ABCD1234"
            });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: accountId=ABCD1234"));
            Assert.That(nameValuePairs, Is.EqualTo(ExpectedBaseNameValuePairs + " accountId=ABCD1234"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TwoValuesWithFormatDetailsAndException()
        {
            //-- Arrange

            var exception = new DivideByZeroException();
            var node = new NameValuePairLogNode<string, decimal>(
                "Test.MessageOne", LogLevel.Info, exception, 
                value1: new LogNameValuePair<string> {
                    Name = "accountId",
                    Value = "ABCD1234"
                },
                value2: new LogNameValuePair<decimal> {
                    Name = "balance",
                    Value = 1234567890m,
                    Format = "#,###.00",
                    IsDetail = true
                });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var fullDetailsText = node.FullDetailsText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: accountId=ABCD1234"));

            Assert.That(fullDetailsText.Contains(exception.ToString()));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " exceptionType=System.DivideByZeroException exception=\"Attempted to divide by zero.\" accountId=ABCD1234 balance=1,234,567,890.00"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreeValuesSingleLineText()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string, decimal, DayOfWeek>(
                "Test.MessageOne", LogLevel.Info, exception: null,
                value1: new LogNameValuePair<string> { Name = "P1", Value = "ABC" },
                value2: new LogNameValuePair<decimal> { Name = "P2", Value = 123m },
                value3: new LogNameValuePair<DayOfWeek> { Name = "P3", Value = DayOfWeek.Monday });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: P1=ABC, P2=123, P3=Monday"));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " P1=ABC P2=123 P3=Monday"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void FourValuesSingleLineText()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string, decimal, DayOfWeek, TimeSpan>(
                "Test.MessageOne", LogLevel.Info, exception: null,
                value1: new LogNameValuePair<string> { Name = "P1", Value = "ABC" },
                value2: new LogNameValuePair<decimal> { Name = "P2", Value = 123m },
                value3: new LogNameValuePair<DayOfWeek> { Name = "P3", Value = DayOfWeek.Monday },
                value4: new LogNameValuePair<TimeSpan> { Name = "P4", Value = TimeSpan.FromHours(1) });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: P1=ABC, P2=123, P3=Monday, P4=01:00:00"));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " P1=ABC P2=123 P3=Monday P4=01:00:00"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void FiveValuesSingleLineText()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string, decimal, DayOfWeek, TimeSpan, DateTime>(
                "Test.MessageOne", LogLevel.Info, exception: null,
                value1: new LogNameValuePair<string> { Name = "P1", Value = "ABC" },
                value2: new LogNameValuePair<decimal> { Name = "P2", Value = 123m },
                value3: new LogNameValuePair<DayOfWeek> { Name = "P3", Value = DayOfWeek.Monday },
                value4: new LogNameValuePair<TimeSpan> { Name = "P4", Value = TimeSpan.FromHours(1) },
                value5: new LogNameValuePair<DateTime> { Name = "P5", Value = new DateTime(2015, 1, 31), Format = "yyyy-MM-dd"});

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: P1=ABC, P2=123, P3=Monday, P4=01:00:00, P5=2015-01-31"));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " P1=ABC P2=123 P3=Monday P4=01:00:00 P5=2015-01-31"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void SixValuesSingleLineText()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string, decimal, DayOfWeek, TimeSpan, DateTime, bool>(
                "Test.MessageOne", LogLevel.Info, exception: null,
                value1: new LogNameValuePair<string> { Name = "P1", Value = "ABC" },
                value2: new LogNameValuePair<decimal> { Name = "P2", Value = 123m },
                value3: new LogNameValuePair<DayOfWeek> { Name = "P3", Value = DayOfWeek.Monday },
                value4: new LogNameValuePair<TimeSpan> { Name = "P4", Value = TimeSpan.FromHours(1) },
                value5: new LogNameValuePair<DateTime> { Name = "P5", Value = new DateTime(2015, 1, 31), Format = "yyyy-MM-dd" },
                value6: new LogNameValuePair<bool> { Name = "P6", Value = true });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: P1=ABC, P2=123, P3=Monday, P4=01:00:00, P5=2015-01-31, P6=True"));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " P1=ABC P2=123 P3=Monday P4=01:00:00 P5=2015-01-31 P6=True"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void SevenValuesSingleLineText()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string, decimal, DayOfWeek, TimeSpan, DateTime, bool, Guid>(
                "Test.MessageOne", LogLevel.Info, exception: null,
                value1: new LogNameValuePair<string> { Name = "P1", Value = "ABC" },
                value2: new LogNameValuePair<decimal> { Name = "P2", Value = 123m },
                value3: new LogNameValuePair<DayOfWeek> { Name = "P3", Value = DayOfWeek.Monday },
                value4: new LogNameValuePair<TimeSpan> { Name = "P4", Value = TimeSpan.FromHours(1) },
                value5: new LogNameValuePair<DateTime> { Name = "P5", Value = new DateTime(2015, 1, 31), Format = "yyyy-MM-dd" },
                value6: new LogNameValuePair<bool> { Name = "P6", Value = true },
                value7: new LogNameValuePair<Guid> { Name = "P7", Value = new Guid("8B8A46A5-FBB1-4258-AB1F-B2EED584D8C9"), Format = "N" });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: P1=ABC, P2=123, P3=Monday, P4=01:00:00, P5=2015-01-31, P6=True, P7=8b8a46a5fbb14258ab1fb2eed584d8c9"));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " P1=ABC P2=123 P3=Monday P4=01:00:00 P5=2015-01-31 P6=True P7=8b8a46a5fbb14258ab1fb2eed584d8c9"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void EightValuesSingleLineText()
        {
            //-- Arrange

            var node = new NameValuePairLogNode<string, decimal, DayOfWeek, TimeSpan, DateTime, bool, string, int>(
                "Test.MessageOne", LogLevel.Info, exception: null,
                value1: new LogNameValuePair<string> { Name = "P1", Value = "ABC" },
                value2: new LogNameValuePair<decimal> { Name = "P2", Value = 123m },
                value3: new LogNameValuePair<DayOfWeek> { Name = "P3", Value = DayOfWeek.Monday },
                value4: new LogNameValuePair<TimeSpan> { Name = "P4", Value = TimeSpan.FromHours(1) },
                value5: new LogNameValuePair<DateTime> { Name = "P5", Value = new DateTime(2015, 1, 31), Format = "yyyy-MM-dd" },
                value6: new LogNameValuePair<bool> { Name = "P6", Value = true },
                value7: new LogNameValuePair<string> { Name = "P7", Value = null },
                value8: new LogNameValuePair<int> { Name = "P8", Value = 1234, Format = "#,###" });

            _threadLog.AppendNode(node);

            //-- Act

            var singleLineText = node.SingleLineText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one: P1=ABC, P2=123, P3=Monday, P4=01:00:00, P5=2015-01-31, P6=True, P7=null, P8=1,234"));

            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " P1=ABC P2=123 P3=Monday P4=01:00:00 P5=2015-01-31 P6=True P7=null P8=1,234"));
        }
    }
}
