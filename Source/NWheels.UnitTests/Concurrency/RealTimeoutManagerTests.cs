using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Concurrency.Impl;
using NWheels.Testing;

namespace NWheels.UnitTests.Concurrency
{
    [TestFixture, Category("System")]
    public class RealTimeoutManagerTests : IntegrationTestWithNodeHosts
    {
        private class Counters
        {
            public int ValidNoParamsCbs { get; set; }
            public int ValidWithParamCbs { get; set; }

            public int InvalidNoParamsCbs { get; set; }
            public int InvalidWithParamCbs { get; set; }

            public int CancelTimeoutCbs { get; set; }
            public int ExceptionsCount { get; set; }
        }

        private Counters _executedCounters;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Test]
        //public void SmokeTest()
        //{
        //    Console.WriteLine("THE FRAMEWORK IS: " + base.AgentComponent.Framework.GetType().FullName);
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestTimeoutFeaturesSequential()
        {

            _executedCounters = new Counters();
            Counters requestedCounters = new Counters();

            CreateEventsSequential(requestedCounters);
            Thread.Sleep(10000);
            AssertEvents(requestedCounters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestTimeoutFeaturesInParallel()
        {

            _executedCounters = new Counters();
            Counters requestedCounters = new Counters();

            CreateEventsInParallel(requestedCounters);
            Thread.Sleep(10000);
            AssertEvents(requestedCounters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool HasDatabase
        {
            get { return false; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertEvents(Counters requestedCounters)
        {
            Assert.AreEqual(0, _executedCounters.InvalidNoParamsCbs);
            Assert.AreEqual(0, _executedCounters.InvalidWithParamCbs);
            Assert.AreEqual(0, _executedCounters.ExceptionsCount);
            Assert.AreEqual(requestedCounters.ValidNoParamsCbs, _executedCounters.ValidNoParamsCbs);
            Assert.AreEqual(requestedCounters.ValidWithParamCbs, _executedCounters.ValidWithParamCbs);
            Assert.AreEqual(requestedCounters.InvalidWithParamCbs + requestedCounters.InvalidNoParamsCbs, _executedCounters.CancelTimeoutCbs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateEventsInParallel(Counters requestedCounters)
        {
            Random rnd = new Random();

            Parallel.For(
                0,
                200000,
                counter => {
                    int action;
                    lock(rnd) { action = rnd.Next(4);}
                    int timeoutInMsec = rnd.Next(150, 4000);
                    CreateNewTimedEvent(requestedCounters, counter, action, timeoutInMsec);
                });
            //for ( int i = 0 ; i < 200000 ; i++ )
            //{
            //    CreateNewTimedEvent(requestedCounters, action, i, timeoutInMsec);
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateEventsSequential(Counters requestedCounters)
        {
            Random rnd = new Random();
            for ( int i = 0 ; i < 200000 ; i++ )
            {
                int action = rnd.Next(4);
                int timeoutInMsec = rnd.Next(150, 4000);
                CreateNewTimedEvent(requestedCounters, action, i, timeoutInMsec);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateNewTimedEvent(Counters requestedCounters, int counter, int action, int timeoutInMsec)
        {
            switch ( action )
            {
                case 0:
                {
                    // Valid action without parameter
                    AgentComponent.Framework.NewTimer("Valid No Param", counter.ToString(), TimeSpan.FromMilliseconds(timeoutInMsec), ValidNoParamCallBack);
                    lock ( requestedCounters )
                    {
                        requestedCounters.ValidNoParamsCbs++;
                    }
                    break;
                }
                case 1:
                {
                    // Valid action with parameter
                    AgentComponent.Framework.NewTimer(
                        "Valid With Param",
                        counter.ToString(),
                        TimeSpan.FromMilliseconds(timeoutInMsec),
                        ValidWithParamCallBack,
                        _executedCounters);
                    lock ( requestedCounters )
                    {
                        requestedCounters.ValidWithParamCbs++;
                    }
                    break;
                }
                case 2:
                {
                    // Invalid action without parameter
                    int factor = timeoutInMsec > 500 ? 2 : 4;
                    ITimeoutHandle toHandle = AgentComponent.Framework.NewTimer(
                        "Invalid No Param",
                        counter.ToString(),
                        TimeSpan.FromMilliseconds(timeoutInMsec * factor),
                        InvalidNoParamCallBack);
                    AgentComponent.Framework.NewTimer("Cancel timer", counter.ToString(), TimeSpan.FromMilliseconds(timeoutInMsec), CancelTimeoutEvent, toHandle);
                    lock ( requestedCounters )
                    {
                        requestedCounters.InvalidNoParamsCbs++;
                    }
                    break;
                }
                case 3:
                {
                    // Invalid action with parameter
                    int factor = timeoutInMsec > 500 ? 2 : 4;
                    ITimeoutHandle toHandle = AgentComponent.Framework.NewTimer(
                        "Invalid With Param",
                        counter.ToString(),
                        TimeSpan.FromMilliseconds(timeoutInMsec * factor),
                        InvalidWithParamCallBack,
                        _executedCounters);
                    AgentComponent.Framework.NewTimer("Cancel timer", counter.ToString(), TimeSpan.FromMilliseconds(timeoutInMsec), CancelTimeoutEvent, toHandle);
                    lock ( requestedCounters )
                    {
                        requestedCounters.InvalidWithParamCbs++;
                    }
                    break;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidNoParamCallBack()
        {
            lock ( _executedCounters )
            {
                _executedCounters.ValidNoParamsCbs++;
            }
        }

        private void ValidWithParamCallBack(Counters counters)
        {
            lock ( counters )
            {
                counters.ValidWithParamCbs++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InvalidNoParamCallBack()
        {
            lock ( _executedCounters )
            {
                _executedCounters.InvalidNoParamsCbs++;
            }
        }

        private void InvalidWithParamCallBack(Counters counters)
        {
            lock ( counters )
            {
                counters.InvalidWithParamCbs++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CancelTimeoutEvent(ITimeoutHandle toHandle)
        {
            try
            {
                toHandle.CancelTimer();
                lock ( _executedCounters )
                {
                    _executedCounters.CancelTimeoutCbs++;
                }
            }
            catch ( Exception )
            {
                lock ( _executedCounters )
                {
                    _executedCounters.ExceptionsCount++;
                }
            }
        }
    }
}

