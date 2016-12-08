using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Concurrency;
using Shouldly;

namespace NWheels.UnitTests.Concurrency
{
    [TestFixture]
    public class SingleProducerConsumerQueueTests
    {
        [Test]
        public void CanEnqueueAndDequeue()
        {
            //- arrane

            var queue = new SingleProducerConsumerQueue<int>(capacity: 100);

            queue.TryEnqueue(111, TimeSpan.MaxValue, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(222, TimeSpan.MaxValue, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(333, TimeSpan.MaxValue, CancellationToken.None).ShouldBe(true);

            //- act & assert

            int dequeued1, dequeued2, dequeued3;
            
            queue.TryDequeue(out dequeued1, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryDequeue(out dequeued2, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryDequeue(out dequeued3, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            dequeued1.ShouldBe(111);
            dequeued2.ShouldBe(222);
            dequeued3.ShouldBe(333);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Test]
        public void CannotDequeueFromInitiallyEmptyQueue()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 100);

            //- act

            int value;
            var wasDequeued = queue.TryDequeue(out value, TimeSpan.FromMilliseconds(50), CancellationToken.None);

            //- assert

            wasDequeued.ShouldBe(false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CannotDequeueFromEmptiedQueue()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 100);
            int dequeuedValue;

            queue.TryEnqueue(111, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(222, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            queue.TryDequeue(out dequeuedValue, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryDequeue(out dequeuedValue, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            //- act

            var wasDequeued = queue.TryDequeue(out dequeuedValue, TimeSpan.FromMilliseconds(50), CancellationToken.None);

            //- assert

            wasDequeued.ShouldBe(false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CannotEnqueueMoreThanCapacity()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);

            queue.TryEnqueue(111, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(222, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(333, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            //- act

            var wasEnqueued = queue.TryEnqueue(444, TimeSpan.FromMilliseconds(50), CancellationToken.None);


            //- assert

            wasEnqueued.ShouldBe(false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void EnqueueToFullBufferBlocksUntilNextDequeue()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);

            queue.TryEnqueue(111, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(222, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(333, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            int dequeuedValue = 0;
            var clock = Stopwatch.StartNew();
            var dequeueTask = Task.Factory.StartNew(() => {
                Thread.Sleep(1000);
                queue.TryDequeue(out dequeuedValue, TimeSpan.Zero, CancellationToken.None);
            });

            //- act

            var wasEnqueued = queue.TryEnqueue(444, TimeSpan.FromSeconds(10), CancellationToken.None);

            //- assert

            wasEnqueued.ShouldBe(true);
            clock.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(1000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void BlockedEnqueueToFullBufferCanBeCanceled()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);

            queue.TryEnqueue(111, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(222, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(333, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            var clock = Stopwatch.StartNew();
            var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            //- act

            var wasEnqueued = queue.TryEnqueue(444, TimeSpan.FromSeconds(10), cancellation.Token);

            //- assert

            wasEnqueued.ShouldBe(false);
            clock.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(500);
            clock.ElapsedMilliseconds.ShouldBeLessThan(2000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void BlockedDequeueFromEmptyBufferCanBeCanceled()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);
            var clock = Stopwatch.StartNew();
            var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            //- act

            int dequeuedValue;
            var wasDequeued = queue.TryDequeue(out dequeuedValue, TimeSpan.FromSeconds(10), cancellation.Token);

            //- assert

            wasDequeued.ShouldBe(false);
            clock.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(500);
            clock.ElapsedMilliseconds.ShouldBeLessThan(2000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void DequeueFromInitiallyEmptyBufferBlocksUntilNextEnqueue()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);

            var clock = Stopwatch.StartNew();
            var enqueueTask = Task.Factory.StartNew(() => {
                Thread.Sleep(1000);
                queue.TryEnqueue(123, TimeSpan.Zero, CancellationToken.None);
            });

            //- act

            int dequeuedValue;
            var wasDequeued = queue.TryDequeue(out dequeuedValue, TimeSpan.FromSeconds(10), CancellationToken.None);

            //- assert

            wasDequeued.ShouldBe(true);
            dequeuedValue.ShouldBe(123);
            clock.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(1000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void DequeueFromEmptiedBufferBlocksUntilNextEnqueue()
        {
            //- arrange

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);
            int dequeuedValue;

            queue.TryEnqueue(111, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryEnqueue(222, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            queue.TryDequeue(out dequeuedValue, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);
            queue.TryDequeue(out dequeuedValue, TimeSpan.Zero, CancellationToken.None).ShouldBe(true);

            var clock = Stopwatch.StartNew();
            var enqueueTask = Task.Factory.StartNew(() => {
                Thread.Sleep(1000);
                queue.TryEnqueue(333, TimeSpan.Zero, CancellationToken.None);
            });

            //- act

            var wasDequeued = queue.TryDequeue(out dequeuedValue, TimeSpan.FromSeconds(10), CancellationToken.None);

            //- assert

            wasDequeued.ShouldBe(true);
            dequeuedValue.ShouldBe(333);           
            clock.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(1000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanProduceAndConsumeInSeparateThreads()
        {
            //- arrange

            const int itemCount = 1000000;

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);
            
            //- act
            
            var producerTask = Task.Factory.StartNew(
                () => {
                    for (int i = 0 ; i < itemCount ; i++)
                    {
                        if (!queue.TryEnqueue(i, TimeSpan.FromMilliseconds(500), CancellationToken.None))
                        {
                            throw new Exception("Could not enqueue item!");
                        }
                    }
                });
            
            var dequeuedValues = new List<int>();

            for (int i = 0 ; i < itemCount ; i++)
            {
                int value;
                if (!queue.TryDequeue(out value, TimeSpan.FromMilliseconds(500), CancellationToken.None))
                {
                    throw new Exception("Could not dequeue item!");
                }

                dequeuedValues.Add(value);
            }

            //- assert

            dequeuedValues.Count.ShouldBe(itemCount);

            for (int i = 0 ; i < dequeuedValues.Count ; i++)
            {
                dequeuedValues[i].ShouldBe(i, "dequeuedValues[" + i + "]");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /*
        [Test]
        public void CanProduceAndConsumeWithAsyncMethods()
        {
            //- arrange

            const int itemCount = 10;//00000;

            var queue = new SingleProducerConsumerQueue<int>(capacity: 3);
            var dequeuedValues = new List<int>();

            Func<Task> producer = async () => {
                for (int i = 0; i < itemCount; i++)
                {
                    if (!await queue.EnqueueAsync(i, TimeSpan.FromMilliseconds(500), CancellationToken.None))
                    {
                        throw new Exception("Could not enqueue item!");
                    }
                    Thread.Sleep(20000);
                }
            };

            Func<Task> consumer = async () => {
                for (int i = 0; i < itemCount; i++)
                {
                    int value;
                    if (!await queue.DequeueAsync(out value, TimeSpan.FromMilliseconds(500), CancellationToken.None))
                    {
                        throw new Exception("Could not dequeue item!");
                    }
                    dequeuedValues.Add(value);
                }
            };

            //- act

            var producerTask = producer();
            var consumerTask = consumer();

            Task.WaitAll(producerTask, consumerTask);

            //- assert

            dequeuedValues.Count.ShouldBe(itemCount);

            for (int i = 0; i < dequeuedValues.Count; i++)
            {
                dequeuedValues[i].ShouldBe(i, "dequeuedValues[" + i + "]");
            }
        }
         * 
         */
    }
}
