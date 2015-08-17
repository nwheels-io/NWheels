using System;
using System.Threading;
using NUnit.Framework;
using NWheels.Concurrency;
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
        private const int TotalEventsToCreate = 200000;

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

            CreateEventsSequential(requestedCounters, TotalEventsToCreate);
            Thread.Sleep(10000);
            AssertEvents(requestedCounters);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestTimeoutFeaturesInParallel()
        {
            _executedCounters = new Counters();
            Counters requestedCounters = new Counters();

            CreateEventsInParallel(requestedCounters, TotalEventsToCreate);
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
            Assertions.Include( () => Assert.AreEqual(0, _executedCounters.InvalidNoParamsCbs, "InvalidNoParamsCbs") );
            Assertions.Include(() => Assert.AreEqual(0, _executedCounters.InvalidWithParamCbs, "InvalidWithParamCbs"));
            Assertions.Include(() => Assert.AreEqual(0, _executedCounters.ExceptionsCount, "ExceptionsCount"));
            Assertions.Include(() => Assert.AreEqual(requestedCounters.ValidNoParamsCbs, _executedCounters.ValidNoParamsCbs, "ValidNoParamsCbs"));
            Assertions.Include(() => Assert.AreEqual(requestedCounters.ValidWithParamCbs, _executedCounters.ValidWithParamCbs, "ValidWithParamCbs"));
            Assertions.Include(() => Assert.AreEqual(requestedCounters.InvalidWithParamCbs + requestedCounters.InvalidNoParamsCbs, _executedCounters.CancelTimeoutCbs, "CancelTimeoutCbs"));

            Assertions.AssertAllSucceeded(Logger);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateEventsInParallel(Counters requestedCounters, int totalEventsToCreate)
        {
            Random rnd = new Random();

            Thread[] threads = new Thread[4];

            for ( int i = 0 ; i < threads.Length ; i++ )
            {
                var counterBase = i;
                Thread t = new Thread(
                    () => {
                        for (int j = 0; j < totalEventsToCreate / threads.Length; j++)
                        {
                            int action = rnd.Next(4);
                            int timeoutInMsec = rnd.Next(150, 4000);
                            CreateNewTimedEvent(requestedCounters, action, (counterBase+1) * j, timeoutInMsec);
                        }
                    });
                threads[i] = t;
                t.Start();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateEventsSequential(Counters requestedCounters, int totalEventsToCreate)
        {
            Random rnd = new Random();
            for ( int i = 0 ; i < totalEventsToCreate ; i++ )
            {
                int action = rnd.Next(4);
                int timeoutInMsec = rnd.Next(150, 4000);
                CreateNewTimedEvent(requestedCounters, action, i, timeoutInMsec);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateNewTimedEvent(Counters requestedCounters, int action, int counter, int timeoutInMsec)
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
                    // Invalid action without parameter (cancelled before triggered)
                    int factor = 2;
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
                    // Invalid action with parameter (cancelled before triggered)
                    int factor = 2;
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

