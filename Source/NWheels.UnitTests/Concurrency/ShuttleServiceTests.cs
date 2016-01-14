using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Concurrency;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Concurrency
{   
    [TestFixture, Category(TestCategory.Integration)]
    public class ShuttleServiceTests : UnitTestBase
    {
        private readonly object _receivedBatchesSyncRoot = new object();
        private List<TestWorkItem[]> _receivedBatches;
        private ShuttleService<TestWorkItem> _shuttleService;
        private List<Thread> _feederThreads;
        private Stopwatch _clock;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _clock = Stopwatch.StartNew();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ShortRunningSingleDriver()
        {
            RunTest(
                numberOfItems: 1000, 
                minFeedDelayMs: 10,
                maxFeedDelayMs: 100,
                feederThreadCount: 5,
                batchSize: 50,
                batchTimeoutMs: 400,
                driverThreadCount: 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void LongRunningSingleDriver()
        {
            RunTest(
                numberOfItems: 10000,
                minFeedDelayMs: 10,
                maxFeedDelayMs: 100,
                feederThreadCount: 5,
                batchSize: 50,
                batchTimeoutMs: 400,
                driverThreadCount: 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ShortRunningMultipleDrivers()
        {
            RunTest(
                numberOfItems: 1000,
                minFeedDelayMs: 10,
                maxFeedDelayMs: 100,
                feederThreadCount: 10,
                batchSize: 50,
                batchTimeoutMs: 250,
                driverThreadCount: 2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void LongRunningMultipleDrivers()
        {
            RunTest(
                numberOfItems: 10000,
                minFeedDelayMs: 10,
                maxFeedDelayMs: 100,
                feederThreadCount: 10,
                batchSize: 50,
                batchTimeoutMs: 250,
                driverThreadCount: 2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunTest(
            int numberOfItems, 
            int minFeedDelayMs, 
            int maxFeedDelayMs, 
            int feederThreadCount, 
            int batchSize, 
            int batchTimeoutMs, 
            int driverThreadCount)
        {
            Console.WriteLine("+ {0} > Starting shuttle service", _clock.Elapsed);

            _shuttleService = new ShuttleService<TestWorkItem>(
                Framework, 
                "TEST", 
                batchSize, 
                TimeSpan.FromMilliseconds(batchTimeoutMs), 
                driverThreadCount,
                OnBatchReceived);

            _receivedBatches = new List<TestWorkItem[]>(capacity: 2 * numberOfItems / batchSize);
            _feederThreads = new List<Thread>();

            _shuttleService.Start();

            Console.WriteLine("+ {0} > Starting feeder threads", _clock.Elapsed);

            TestWorkItem.ResetLastId();

            for ( int i = 0 ; i < feederThreadCount ; i++ )
            {
                CreateFeederThread(numberOfItems, minFeedDelayMs, maxFeedDelayMs, threadIndex: i + 1);
            }

            foreach ( var thread in _feederThreads )
            {
                thread.Start();
            }

            Console.WriteLine("+ {0} > Waiting for feeder threads to exit", _clock.Elapsed);

            foreach ( var thread in _feederThreads )
            {
                thread.Join();
            }

            //Thread.Sleep(3000);

            Console.WriteLine("+ {0} > Stopping shuttle service", _clock.Elapsed);

            _shuttleService.Stop(TimeSpan.FromSeconds(5));

            Console.WriteLine("+ {0} > Analyzing results", _clock.Elapsed);

            var totalItemsReceived = _receivedBatches.Sum(b => b.Length);
            var totalUniqueItemsReceived = _receivedBatches.SelectMany(b => b.Select(item => item.Id)).Distinct().Count();
            
            var maxBatchSize = _receivedBatches.Max(b => b.Length);
            var minBatchSize = _receivedBatches.Min(b => b.Length);
            var avgBatchSize = _receivedBatches.Average(b => b.Length);

            var maxCompletionTimeMs = _receivedBatches.SelectMany(b => b.Select(item => (int)item.GetCompletionTime().TotalMilliseconds)).Max();
            var minCompletionTimeMs = _receivedBatches.SelectMany(b => b.Select(item => (int)item.GetCompletionTime().TotalMilliseconds)).Min();
            var avgCompletionTimeMs = _receivedBatches.SelectMany(b => b.Select(item => (int)item.GetCompletionTime().TotalMilliseconds)).Average();

            Console.WriteLine("------ BATCHES:    {0} received, {1} max size, {2} min size, {3} avg. size", _receivedBatches.Count, maxBatchSize, minBatchSize, avgBatchSize);
            Console.WriteLine("------ ITEMS:      {0} total, {1} unique", totalItemsReceived, totalUniqueItemsReceived);
            Console.WriteLine("------ COMPLETION: {0} max, {1} min, {2} avg", maxCompletionTimeMs, minCompletionTimeMs, avgCompletionTimeMs);

            totalItemsReceived.ShouldBe(numberOfItems);
            totalUniqueItemsReceived.ShouldBe(numberOfItems);
            maxBatchSize.ShouldBeLessThanOrEqualTo(batchSize);
            minBatchSize.ShouldBeGreaterThan(0);
            maxCompletionTimeMs.ShouldBeLessThanOrEqualTo(batchTimeoutMs + 100);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateFeederThread(int numberOfItems, int minFeedDelayMs, int maxFeedDelayMs, int threadIndex)
        {
            _feederThreads.Add(new Thread(() => {
                var random = new Random(threadIndex);
                while ( true )
                {
                    var delayMs = random.Next(minFeedDelayMs, maxFeedDelayMs);
                    Thread.Sleep(delayMs);
                    var workItem = new TestWorkItem(_clock);
                    if ( workItem.Id > numberOfItems )
                    {
                        Console.WriteLine("+ {0} > Feeder thread #{1} - DONE.", _clock.Elapsed, threadIndex);
                        break;
                    }
                    if ( (workItem.Id % 100) == 0 )
                    {
                        Console.WriteLine("+ {0} > {1} work items boarded (feeder thread #{2})", _clock.Elapsed, workItem.Id, threadIndex);
                    }
                    _shuttleService.Board(workItem);
                }
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnBatchReceived(TestWorkItem[] batch)
        {
            var now = _clock.Elapsed;

            lock ( _receivedBatchesSyncRoot )
            {
                _receivedBatches.Add(batch);
            }

            for ( int i = 0 ; i < batch.Length ; i++ )
            {
                batch[i].CompletedAt = now;
            }

            var minCompletion = batch.Min(item => (int)item.GetCompletionTime().TotalMilliseconds);
            var maxCompletion = batch.Max(item => (int)item.GetCompletionTime().TotalMilliseconds);

            Console.WriteLine(
                "+ {0} > Batch received: size={1}, ids={2}..{3}, completion={4}..{5}", 
                now, batch.Length, batch[0].Id, batch[batch.Length - 1].Id, minCompletion, maxCompletion);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestWorkItem
        {
            public TestWorkItem(Stopwatch clock)
            {
                this.Id = Interlocked.Increment(ref _s_lastId);
                this.CreatedAt = clock.Elapsed;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TimeSpan GetCompletionTime()
            {
                if ( CompletedAt.HasValue )
                {
                    return CompletedAt.Value.Subtract(CreatedAt);
                }
                else
                {
                    return TimeSpan.MaxValue;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Id { get; private set; }
            public TimeSpan CreatedAt { get; private set; }
            public TimeSpan? CompletedAt { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static int _s_lastId = 0;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void ResetLastId()
            {
                _s_lastId = 0;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static int GetLastId()
            {
                return _s_lastId;
            }
        }
    }
}
