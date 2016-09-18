using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Logging;
using NWheels.Logging.Core;
using Shouldly;

namespace NWheels.UnitTests.Logging
{
    [TestFixture]
    public class AggregationTests : ThreadLogUnitTestBase
    {
        private ThreadLog _threadLog;
        private NameValuePairActivityLogNode _rootActivity;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void Setup()
        {
            _rootActivity = new NameValuePairActivityLogNode("Test.Root", LogLevel.Verbose, LogOptions.None);
            _threadLog = new ThreadLog(Framework, Clock, Registry, Anchor, ThreadTaskType.Unspecified, _rootActivity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAggregateChildren()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            //-- act

            using (NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.Aggregate)))
            {
                Clock.ElapsedMilliseconds = 150; // 50 ms duraion
            }

            using (NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.Aggregate)))
            {
                Clock.ElapsedMilliseconds = 180; // 30 ms duration
            }

            using (NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.Aggregate)))
            {
                Clock.ElapsedMilliseconds = 300; // 120 ms duration
            }

            LogTotal[] totals = _rootActivity.GetTotals(includeBuiltIn: false);

            //-- assert

            totals.Length.ShouldBe(2);
            
            totals[0].MessageId.ShouldBe("M1");
            totals[0].Count.ShouldBe(2);
            totals[0].MicrosecondsDuration.ShouldBe(170 * 1000);

            totals[1].MessageId.ShouldBe("M2");
            totals[1].Count.ShouldBe(1);
            totals[1].MicrosecondsDuration.ShouldBe(30 * 1000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanBubbleAggregationsToParent()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            ActivityLogNode parent1, parent2;

            //-- act

            using (parent1 = NewActivity(new NameValuePairActivityLogNode("P1", LogLevel.Verbose, LogOptions.None)))
            {
                using (NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.Aggregate)))
                {
                    Clock.ElapsedMilliseconds = 150; // 50 ms duraion
                }

                using (NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.Aggregate)))
                {
                    Clock.ElapsedMilliseconds = 180; // 30 ms duration
                }
            }

            using (parent2 = NewActivity(new NameValuePairActivityLogNode("P2", LogLevel.Verbose, LogOptions.None)))
            {
                using (NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.Aggregate)))
                {
                    Clock.ElapsedMilliseconds = 220; // 40 ms duration
                }
            }

            LogTotal[] rootTotals = _rootActivity.GetTotals(includeBuiltIn: false);
            LogTotal[] parent1Totals = parent1.GetTotals(includeBuiltIn: false);
            LogTotal[] parent2Totals = parent2.GetTotals(includeBuiltIn: false);

            //-- assert

            rootTotals.Length.ShouldBe(2);

            rootTotals[0].MessageId.ShouldBe("M1");
            rootTotals[0].Count.ShouldBe(1);
            rootTotals[0].MicrosecondsDuration.ShouldBe(50 * 1000);

            rootTotals[1].MessageId.ShouldBe("M2");
            rootTotals[1].Count.ShouldBe(2);
            rootTotals[1].MicrosecondsDuration.ShouldBe(70 * 1000);

            parent1Totals.Length.ShouldBe(2);

            parent1Totals[0].MessageId.ShouldBe("M1");
            parent1Totals[0].Count.ShouldBe(1);
            parent1Totals[0].MicrosecondsDuration.ShouldBe(50 * 1000);

            parent1Totals[1].MessageId.ShouldBe("M2");
            parent1Totals[1].Count.ShouldBe(1);
            parent1Totals[1].MicrosecondsDuration.ShouldBe(30 * 1000);

            parent2Totals.Length.ShouldBe(1);

            parent2Totals[0].MessageId.ShouldBe("M2");
            parent2Totals[0].Count.ShouldBe(1);
            parent2Totals[0].MicrosecondsDuration.ShouldBe(40 * 1000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAggregateLogMessages()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            ActivityLogNode parent;

            //-- act

            using (parent = NewActivity(new NameValuePairActivityLogNode("P1", LogLevel.Verbose, LogOptions.None)))
            {
                _threadLog.AppendNode(new NameValuePairLogNode("M1", LogLevel.Verbose, LogOptions.Aggregate, exception: null));
                _threadLog.AppendNode(new NameValuePairLogNode("M2", LogLevel.Verbose, LogOptions.Aggregate, exception: null));
                _threadLog.AppendNode(new NameValuePairLogNode("M2", LogLevel.Verbose, LogOptions.Aggregate, exception: null));
            }

            LogTotal[] rootTotals = _rootActivity.GetTotals(includeBuiltIn: false);
            LogTotal[] parentTotals = parent.GetTotals(includeBuiltIn: false);

            //-- assert

            rootTotals.Length.ShouldBe(2);

            rootTotals[0].MessageId.ShouldBe("M1");
            rootTotals[0].Count.ShouldBe(1);
            rootTotals[0].MicrosecondsDuration.ShouldBe(0);

            rootTotals[1].MessageId.ShouldBe("M2");
            rootTotals[1].Count.ShouldBe(2);
            rootTotals[1].MicrosecondsDuration.ShouldBe(0);

            parentTotals.Length.ShouldBe(2);

            parentTotals[0].MessageId.ShouldBe("M1");
            parentTotals[0].Count.ShouldBe(1);
            parentTotals[0].MicrosecondsDuration.ShouldBe(0);

            parentTotals[1].MessageId.ShouldBe("M2");
            parentTotals[1].Count.ShouldBe(2);
            parentTotals[1].MicrosecondsDuration.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAggregateDbAccess()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            ActivityLogNode parent1, parent2;
            ActivityLogNode child1, child2, child3;

            //-- act

            using (parent1 = NewActivity(new NameValuePairActivityLogNode("P1", LogLevel.Verbose, LogOptions.None)))
            {
                using (child1 = NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.AggregateAsDbAccess)))
                {
                    Clock.ElapsedMilliseconds = 110; // 10 ms duraion
                }

                using (child2 = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsDbAccess)))
                {
                    Clock.ElapsedMilliseconds = 130; // 20 ms duraion
                }
            }

            using (parent2 = NewActivity(new NameValuePairActivityLogNode("P2", LogLevel.Verbose, LogOptions.None)))
            {
                using (child3 = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsDbAccess)))
                {
                    Clock.ElapsedMilliseconds = 170; // 40 ms duraion
                }
            }

            //-- assert

            child1.DbTotal.Count.ShouldBe(1);
            child1.DbTotal.MicrosecondsDuration.ShouldBe(10 * 1000);

            child2.DbTotal.Count.ShouldBe(1);
            child2.DbTotal.MicrosecondsDuration.ShouldBe(20 * 1000);

            parent1.DbTotal.Count.ShouldBe(2);
            parent1.DbTotal.MicrosecondsDuration.ShouldBe(30 * 1000);

            parent1.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent1.CommunicationTotal.Count.ShouldBe(0);
            parent1.LockWaitTotal.Count.ShouldBe(0);
            parent1.LockHoldTotal.Count.ShouldBe(0);

            child3.DbTotal.Count.ShouldBe(1);
            child3.DbTotal.MicrosecondsDuration.ShouldBe(40 * 1000);

            parent2.DbTotal.Count.ShouldBe(1);
            parent2.DbTotal.MicrosecondsDuration.ShouldBe(40 * 1000);

            parent2.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent2.CommunicationTotal.Count.ShouldBe(0);
            parent2.LockWaitTotal.Count.ShouldBe(0);
            parent2.LockHoldTotal.Count.ShouldBe(0);

            _rootActivity.DbTotal.Count.ShouldBe(3);
            _rootActivity.DbTotal.MicrosecondsDuration.ShouldBe(70 * 1000);

            _rootActivity.GetTotals(includeBuiltIn: false).ShouldBeNull();
            _rootActivity.CommunicationTotal.Count.ShouldBe(0);
            _rootActivity.LockWaitTotal.Count.ShouldBe(0);
            _rootActivity.LockHoldTotal.Count.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAggregateCommunication()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            ActivityLogNode parent1, parent2;

            //-- act

            using (parent1 = NewActivity(new NameValuePairActivityLogNode("P1", LogLevel.Verbose, LogOptions.None)))
            {
                using (var child = NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.AggregateAsCommunication)))
                {
                    Clock.ElapsedMilliseconds = 110; // 10 ms duraion
                }

                using (var child = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsCommunication)))
                {
                    Clock.ElapsedMilliseconds = 130; // 20 ms duraion
                }
            }

            using (parent2 = NewActivity(new NameValuePairActivityLogNode("P2", LogLevel.Verbose, LogOptions.None)))
            {
                using (var child = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsCommunication)))
                {
                    Clock.ElapsedMilliseconds = 170; // 40 ms duraion
                }
            }

            //-- assert

            parent1.CommunicationTotal.Count.ShouldBe(2);
            parent1.CommunicationTotal.MicrosecondsDuration.ShouldBe(30 * 1000);

            parent1.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent1.DbTotal.Count.ShouldBe(0);
            parent1.LockWaitTotal.Count.ShouldBe(0);
            parent1.LockHoldTotal.Count.ShouldBe(0);

            parent2.CommunicationTotal.Count.ShouldBe(1);
            parent2.CommunicationTotal.MicrosecondsDuration.ShouldBe(40 * 1000);

            parent2.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent2.DbTotal.Count.ShouldBe(0);
            parent2.LockWaitTotal.Count.ShouldBe(0);
            parent2.LockHoldTotal.Count.ShouldBe(0);

            _rootActivity.CommunicationTotal.Count.ShouldBe(3);
            _rootActivity.CommunicationTotal.MicrosecondsDuration.ShouldBe(70 * 1000);

            _rootActivity.GetTotals(includeBuiltIn: false).ShouldBeNull();
            _rootActivity.DbTotal.Count.ShouldBe(0);
            _rootActivity.LockWaitTotal.Count.ShouldBe(0);
            _rootActivity.LockHoldTotal.Count.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAggregateLockWait()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            ActivityLogNode parent1, parent2;

            //-- act

            using (parent1 = NewActivity(new NameValuePairActivityLogNode("P1", LogLevel.Verbose, LogOptions.None)))
            {
                using (var child = NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.AggregateAsLockWait)))
                {
                    Clock.ElapsedMilliseconds = 110; // 10 ms duraion
                }

                using (var child = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsLockWait)))
                {
                    Clock.ElapsedMilliseconds = 130; // 20 ms duraion
                }
            }

            using (parent2 = NewActivity(new NameValuePairActivityLogNode("P2", LogLevel.Verbose, LogOptions.None)))
            {
                using (var child = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsLockWait)))
                {
                    Clock.ElapsedMilliseconds = 170; // 40 ms duraion
                }
            }

            //-- assert

            parent1.LockWaitTotal.Count.ShouldBe(2);
            parent1.LockWaitTotal.MicrosecondsDuration.ShouldBe(30 * 1000);

            parent1.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent1.DbTotal.Count.ShouldBe(0);
            parent1.CommunicationTotal.Count.ShouldBe(0);
            parent1.LockHoldTotal.Count.ShouldBe(0);

            parent2.LockWaitTotal.Count.ShouldBe(1);
            parent2.LockWaitTotal.MicrosecondsDuration.ShouldBe(40 * 1000);

            parent2.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent2.DbTotal.Count.ShouldBe(0);
            parent2.CommunicationTotal.Count.ShouldBe(0);
            parent2.LockHoldTotal.Count.ShouldBe(0);

            _rootActivity.LockWaitTotal.Count.ShouldBe(3);
            _rootActivity.LockWaitTotal.MicrosecondsDuration.ShouldBe(70 * 1000);

            _rootActivity.GetTotals(includeBuiltIn: false).ShouldBeNull();
            _rootActivity.DbTotal.Count.ShouldBe(0);
            _rootActivity.CommunicationTotal.Count.ShouldBe(0);
            _rootActivity.LockHoldTotal.Count.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAggregateLockHold()
        {
            //-- arrange

            Clock.ElapsedMilliseconds = 100;

            ActivityLogNode parent1, parent2;

            //-- act

            using (parent1 = NewActivity(new NameValuePairActivityLogNode("P1", LogLevel.Verbose, LogOptions.None)))
            {
                using (var child = NewActivity(new NameValuePairActivityLogNode("M1", LogLevel.Verbose, LogOptions.AggregateAsLockHold)))
                {
                    Clock.ElapsedMilliseconds = 110; // 10 ms duraion
                }

                using (var child = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsLockHold)))
                {
                    Clock.ElapsedMilliseconds = 130; // 20 ms duraion
                }
            }

            using (parent2 = NewActivity(new NameValuePairActivityLogNode("P2", LogLevel.Verbose, LogOptions.None)))
            {
                using (var child = NewActivity(new NameValuePairActivityLogNode("M2", LogLevel.Verbose, LogOptions.AggregateAsLockHold)))
                {
                    Clock.ElapsedMilliseconds = 170; // 40 ms duraion
                }
            }

            //-- assert

            parent1.LockHoldTotal.Count.ShouldBe(2);
            parent1.LockHoldTotal.MicrosecondsDuration.ShouldBe(30 * 1000);

            parent1.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent1.DbTotal.Count.ShouldBe(0);
            parent1.CommunicationTotal.Count.ShouldBe(0);
            parent1.LockWaitTotal.Count.ShouldBe(0);

            parent2.LockHoldTotal.Count.ShouldBe(1);
            parent2.LockHoldTotal.MicrosecondsDuration.ShouldBe(40 * 1000);

            parent2.GetTotals(includeBuiltIn: false).ShouldBeNull();
            parent2.DbTotal.Count.ShouldBe(0);
            parent2.CommunicationTotal.Count.ShouldBe(0);
            parent2.LockWaitTotal.Count.ShouldBe(0);

            _rootActivity.LockHoldTotal.Count.ShouldBe(3);
            _rootActivity.LockHoldTotal.MicrosecondsDuration.ShouldBe(70 * 1000);

            _rootActivity.GetTotals(includeBuiltIn: false).ShouldBeNull();
            _rootActivity.DbTotal.Count.ShouldBe(0);
            _rootActivity.CommunicationTotal.Count.ShouldBe(0);
            _rootActivity.LockWaitTotal.Count.ShouldBe(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ActivityLogNode NewActivity(ActivityLogNode activity)
        {
            _threadLog.AppendNode(activity);
            return activity;
        }
    }
}
