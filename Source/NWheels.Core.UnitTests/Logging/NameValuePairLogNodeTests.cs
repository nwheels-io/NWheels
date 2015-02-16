using NUnit.Framework;
using NWheels.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Core.UnitTests.Logging
{
    [TestFixture]
    public class NameValuePairLogNodeTests : ThreadLogUnitTestBase
    {
        private const string TestLogId = "e99f7886838e4c37b433888132ef5f86";
        private const string ExpectedBaseNameValuePairs = "message=Test.MessageOne level=Info logid=e99f7886838e4c37b433888132ef5f86";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLog _threadLog;
        private FormattedActivityLogNode _rootActivity;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
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
            var fullDetailsText = node.FullDetailsText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one"));
            Assert.That(fullDetailsText, Is.EqualTo(""));
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
            Assert.That(fullDetailsText, Is.EqualTo(exception.ToString()));
            Assert.That(nameValuePairs, Is.EqualTo(ExpectedBaseNameValuePairs + " exception=System.DivideByZeroException"));
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
            var fullDetailsText = node.FullDetailsText;
            var nameValuePairs = node.NameValuePairsText;

            //-- Assert

            Assert.That(singleLineText, Is.EqualTo("Message one, accountId=ABCD1234"));
            Assert.That(fullDetailsText, Is.EqualTo(""));
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

            Assert.That(singleLineText, Is.EqualTo("Message one, accountId=ABCD1234"));
            
            Assert.That(fullDetailsText, Is.EqualTo(
                "balance=1,234,567,890.00" + System.Environment.NewLine + 
                exception.ToString()));
            
            Assert.That(nameValuePairs, Is.EqualTo(
                ExpectedBaseNameValuePairs +
                " exception=System.DivideByZeroException accountId=ABCD1234 balance=1,234,567,890.00"));
        }
    }
}
