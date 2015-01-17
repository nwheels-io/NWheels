using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using NWheels.Core.Logging;
using NWheels.Logging;
using NWheels.Testing;

namespace NWheels.Core.UnitTests.Logging
{
    [TestFixture]
    public class ThreadLogTests : UnitTestBase
    {
        private TestClock _clock;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _clock = new TestClock();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NewInstance_StartedAtUtc_EqualsUtcNow()
        {
            //-- Arrange

            var now = new DateTime(2014, 10, 15, 12, 30, 45);
            Framework.UtcNow = now;

            //-- Act

            var log = new ThreadLog(
                Framework, _clock, Resolve<ThreadLogRegistry>(), ThreadTaskType.Unspecified, new FormattedActivityLogNode("Test"));

            //-- Assert

            Assert.That(log.ThreadStartedAtUtc, Is.EqualTo(now));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NewInstance_CorrelationId_NewGuid()
        {
            //-- Arrange

            var newGuid = new Guid("E690328B-994E-494F-B4F6-B317A0E2668B");
            Framework.PresetGuids.Enqueue(newGuid);

            //-- Act

            var log = new ThreadLog(
                Framework, _clock, Resolve<ThreadLogRegistry>(), ThreadTaskType.Unspecified, new FormattedActivityLogNode("Test"));

            //-- Assert

            Assert.That(log.CorrelationId, Is.EqualTo(newGuid));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NewInstance_TaskType_AsSpecified()
        {
            //-- Act

            var log = new ThreadLog(
                Framework, _clock, Resolve<ThreadLogRegistry>(), ThreadTaskType.QueuedWorkItem, new FormattedActivityLogNode("Test"));

            //-- Assert

            Assert.That(log.TaskType, Is.EqualTo(ThreadTaskType.QueuedWorkItem));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NewInstance_CurrentActivity_IsRootActivity()
        {
            //-- Arrange

            var rootActivity = new FormattedActivityLogNode("Root");

            //-- Act

            var log = new ThreadLog(
                Framework, _clock, Resolve<ThreadLogRegistry>(), ThreadTaskType.Unspecified, rootActivity);

            //-- Assert

            Assert.That(log.RootActivity, Is.SameAs(rootActivity));
            Assert.That(log.CurrentActivity, Is.SameAs(rootActivity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AppendPlainNode_PlacedUnderCurrentActivity()
        {
            //-- Arrange

            var log = CreateThreadLog();

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AppendActivityNode_NodeBecomesCurrentActivity()
        {
            //-- Arrange

            var log = CreateThreadLog();

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One;AI:Two{LI:Three}}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CloseActivityNode_ParentBecomesCurrentActivity()
        {
            //-- Arrange

            var log = CreateThreadLog();
            ActivityLogNode activity;

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(activity = new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            activity.Close();
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Four"));

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One;AI:Two{LI:Three};LI:Four}"));
            Assert.That(log.CurrentActivity, Is.SameAs(log.RootActivity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AppendWarningLog_BubbleUpToParent()
        {
            //-- Arrange

            var log = CreateThreadLog();

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            log.AppendNode(new FormattedLogNode(LogLevel.Warning, "Four"));

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One;AW:Two{LI:Three;LW:Four}}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CloseWarningActivity_BubbleUpToParent()
        {
            //-- Arrange

            var log = CreateThreadLog();
            ActivityLogNode activityTwo;
            ActivityLogNode activityFour;

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(activityTwo = new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            log.AppendNode(activityFour = new FormattedActivityLogNode("Four"));
            log.AppendNode(new FormattedLogNode(LogLevel.Warning, "Five"));

            activityFour.Close();
            var afterFourClosed = ToTestString(log.RootActivity);

            activityTwo.Close();
            var afterTwoClosed = ToTestString(log.RootActivity);

            //-- Assert

            Assert.That(afterFourClosed, Is.EqualTo("AI:Root{LI:One;AW:Two{LI:Three;AW:Four{LW:Five}}}"));
            Assert.That(afterTwoClosed, Is.EqualTo("AW:Root{LI:One;AW:Two{LI:Three;AW:Four{LW:Five}}}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AppendErrorNodeWithException_SetParentActivityToFailure()
        {
            //-- Arrange

            var log = CreateThreadLog();

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            log.AppendNode(new FormattedActivityLogNode("Four"));
            log.AppendNode(new FormattedLogNode(LogLevel.Error, "Five", exception: new Exception()));

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One;AI:Two{LI:Three;AEX:Four{LEX:Five}}}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CloseFailedActivity_BubbleFailureToParent()
        {
            //-- Arrange

            var log = CreateThreadLog();
            ActivityLogNode activityFour;

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            log.AppendNode(activityFour = new FormattedActivityLogNode("Four"));
            log.AppendNode(new FormattedLogNode(LogLevel.Error, "Five", exception: new Exception()));

            activityFour.Close();

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One;AEX:Two{LI:Three;AEX:Four{LEX:Five}}}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void AppendErrorNodeWithException_ClearFailure_FailureBubbledUpAsWarning()
        {
            //-- Arrange

            var log = CreateThreadLog();
            ActivityLogNode activityFour;

            //-- Act

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            log.AppendNode(activityFour = new FormattedActivityLogNode("Four"));
            log.AppendNode(new FormattedLogNode(LogLevel.Error, "Five", exception: new Exception()), clearFailure: true);

            activityFour.Close();

            //-- Assert

            Assert.That(ToTestString(log.RootActivity), Is.EqualTo("AI:Root{LI:One;AW:Two{LI:Three;AW:Four{LEX:Five}}}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ActivityNotClosed_Duration_FromThreadClock()
        {
            //-- Arrange

            var log = CreateThreadLog();
            ActivityLogNode activity;

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(activity = new FormattedActivityLogNode("Two"));

            //-- Act

            _clock.ElapsedMilliseconds = 123;

            var rootDuration1 = log.RootActivity.MillisecondsDuration;
            var childDuration1 = activity.MillisecondsDuration;

            _clock.ElapsedMilliseconds = 456;

            var rootDuration2 = log.RootActivity.MillisecondsDuration;
            var childDuration2 = activity.MillisecondsDuration;

            //-- Assert

            Assert.That(rootDuration1, Is.EqualTo(123));
            Assert.That(childDuration1, Is.EqualTo(123));

            Assert.That(rootDuration2, Is.EqualTo(456));
            Assert.That(childDuration2, Is.EqualTo(456));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ActivityClosed_Duration_Fixed()
        {
            //-- Arrange

            var log = CreateThreadLog();
            ActivityLogNode activity;

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(activity = new FormattedActivityLogNode("Two"));

            //-- Act

            _clock.ElapsedMilliseconds = 123;

            activity.Close();

            _clock.ElapsedMilliseconds = 456;

            var rootDuration = log.RootActivity.MillisecondsDuration;
            var childDuration = activity.MillisecondsDuration;

            //-- Assert

            Assert.That(rootDuration, Is.EqualTo(456));
            Assert.That(childDuration, Is.EqualTo(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadLogSnapshot_Serialize_Deserialize()
        {
            //-- Arrange

            var log = CreateThreadLog();

            log.AppendNode(new FormattedLogNode(LogLevel.Info, "One"));
            log.AppendNode(new FormattedActivityLogNode("Two"));
            log.AppendNode(new FormattedLogNode(LogLevel.Info, "Three"));
            log.AppendNode(new FormattedActivityLogNode("Four"));
            log.AppendNode(new FormattedLogNode(LogLevel.Error, "Five", exception: new Exception()));

            var originalSnapshot = log.TakeSnapshot();
            var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));
            
            //-- Act

            var output1 = new StringBuilder();
            var writer1 = XmlWriter.Create(output1, new XmlWriterSettings { Indent = true, IndentChars = "\t" });
            serializer.WriteObject(writer1, originalSnapshot);
            writer1.Flush();

            var reader = XmlReader.Create(new StringReader(output1.ToString()));
            var deserializedSnapshot = serializer.ReadObject(reader);

            var output2 = new StringBuilder();
            var writer2 = XmlWriter.Create(output2, new XmlWriterSettings { Indent = true, IndentChars = "\t" });
            serializer.WriteObject(writer2, originalSnapshot);
            writer2.Flush();

            //-- Assert

            Console.WriteLine(output1.ToString());
            Assert.That(output2.ToString(), Is.EqualTo(output1.ToString()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ThreadLog CreateThreadLog(string rootActivityText = "Root", ThreadTaskType taskType = ThreadTaskType.Unspecified)
        {
            var rootActivity = new FormattedActivityLogNode(rootActivityText);
            return new ThreadLog(Framework, _clock, Resolve<ThreadLogRegistry>(), ThreadTaskType.Unspecified, rootActivity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string ToTestString(ActivityLogNode activity)
        {
            var output = new StringBuilder();
            AppendActivityTestString(activity, output);
            return output.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AppendActivityTestString(ActivityLogNode activity, StringBuilder output)
        {
            AppendNodeTestString(activity, output);
            output.Append("{");

            for ( var child = activity.FirstChild ; child != null ; child = child.NextSibling )
            {
                if ( child is ActivityLogNode )
                {
                    AppendActivityTestString((ActivityLogNode)child, output);
                }
                else
                {
                    AppendNodeTestString(child, output);
                }

                if ( child.NextSibling != null )
                {
                    output.Append(";");
                }
            }

            output.Append("}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AppendNodeTestString(LogNode node, StringBuilder output)
        {
            output.Append(node is ActivityLogNode ? "A" : "L");
            output.Append(node.Level.ToString().Substring(0, 1));

            if ( node.Exception != null )
            {
                output.Append("X");
            }

            output.Append(":");
            output.Append(node.SingleLineText);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestClock : IClock
        {
            public long ElapsedMilliseconds { get; set; }
        }
    }
}
