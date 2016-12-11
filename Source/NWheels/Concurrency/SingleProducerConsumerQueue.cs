using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    //
    // Single producer/single consumer lock-free FIFO queue, based on ring buffer and spin-waits.
    //
    internal class SingleProducerConsumerQueue<T>
    {
        // lenth of the ring buffer
        private readonly int _capacity;

        // the ring buffer
        private readonly T[] _buffer;

        // the ring buffer
        private readonly Stoplight[] _producerStoplights;

        // the ring buffer
        private readonly Stoplight[] _consumerStoplights;

        // tracks enqueue operation timeout; 
        // one instance is reused by all Enqueue operations
        private readonly Stopwatch _enqueueClock;

        // tracks dequeue operation timeout; 
        // one instance is reused by all Dequeue operations
        private readonly Stopwatch _dequeueClock;

        // 
        private readonly CompletedDequeueAwaiter _completedDequeueAwaiter;
        // 
        private readonly PendingDequeueAwaiter _pendingDequeueAwaiter;

        // 
        private readonly CompletedEnqueueAwaiter _completedEnqueueAwaiter;
        // 
        private readonly PendingEnqueueAwaiter _pendingEnqueueAwaiter;

        // position of last enqueued item, initially -1
        // it is ever-increasing: modulus capacity to determine actual index in buffer; 
        // this field is only mutated by a single producer thread
        private long _head;

        // position of last dequeued item, initially -1
        // it is ever-increasing: modulus capacity to determine actual index in buffer;
        // this field is only mutated by a single consumer thread
        private long _tail;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        //
        // Initializes new instance of the queue with specified capacity of the ring buiffer.
        //
        public SingleProducerConsumerQueue(int capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
            _producerStoplights = new Stoplight[capacity];
            _consumerStoplights = new Stoplight[capacity];

            for (int i = 0 ; i < capacity ; i++)
            {
                _producerStoplights[i] = Stoplight.Green;
                _consumerStoplights[i] = Stoplight.Green;
            }

            _enqueueClock = Stopwatch.StartNew();
            _dequeueClock = Stopwatch.StartNew();

            _completedEnqueueAwaiter = new CompletedEnqueueAwaiter();
            _pendingEnqueueAwaiter = new PendingEnqueueAwaiter(this);
            _completedDequeueAwaiter = new CompletedDequeueAwaiter();
            _pendingDequeueAwaiter = new PendingDequeueAwaiter(this);

            _head = -1; // first enqueued item will be at position 0
            _tail = -1; // first dequeued item will be at position 0
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        //
        // Enqueues an item into the queue.
        // This method blocks until a slot in buffer is available, timeout expires, or cancellation token is signaled.
        // Returns true if an item was successfully dequeued; false if either timeout expired or cancellation signaled.
        //
        public bool TryEnqueue(T item, TimeSpan timeout, CancellationToken cancel)
        {
            var spin = new SpinWait();

            // check if we have space in buffer for another item
            if (_head - _tail >= _capacity)
            {
                // the buffer is full. we have to wait until the consumer dequeues at least one item
                // use combination of spin-wait/yield (implemented by SpinWait struct) until at least one slot becomes available

                var startTime = _enqueueClock.Elapsed;
                var spinCount = 0;

                while (_head - _tail >= _capacity)
                {
                    spinCount++;
                    spin.SpinOnce(); // each time SpinOnce will decide to either spin or yield

                    // timeout/cancellation check costs some CPU cycles
                    // because of that, we check for timeout/cancellation once every 100 iterations
                    if ((spinCount % 100) == 0 && (cancel.IsCancellationRequested || (_dequeueClock.Elapsed.Subtract(startTime) >= timeout)))
                    {
                        // we've either timed out or canceled.
                        return false;
                    }
                }
            }

            // now we are sure we have space for the new item
            _buffer[(_head + 1) % _capacity] = item;

            // now it is safe to let Dequeue read the enqueued slot, so we increment the _head
            // there is no race condition in incrementing the _head; we use Interlocked to prevent instruction reordering optimizations
            Interlocked.Increment(ref _head);

            // handle potential race conditions when the queue was previously empty and an async dequeue operation is in progress.
            HandlePostEnqueueRaceWithAsyncDequeue(item, spin);

            return true;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        //
        // Dequeues an item from the queue.
        // This method blocks until an item is available, timeout expires, or cancellation is signaled.
        // Returns true if an item was successfully enqueued; false if either timeout expired or cancellation signaled.
        //
        public bool TryDequeue(out T item, TimeSpan timeout, CancellationToken cancel)
        {
            var spin = new SpinWait();
            
            // check if we have at least one item in the queue
            if (_tail >= _head)
            {
                // the buffer is empty. we have to wait until the producer enqueues at least one item
                // use combination of spin-wait/yield (implemented by SpinWait struct) until at least one item  becomes available

                var startTime = _dequeueClock.Elapsed;
                var spinCount = 0;

                while (_tail >= _head)
                {
                    spinCount++;
                    spin.SpinOnce(); // each time SpinOnce will decide to either spin or yield

                    // timeout/cancellation check costs some CPU cycles
                    // because of that, we check for timeout/cancellation once every 100 iterations
                    if ((spinCount % 100) == 0 && (cancel.IsCancellationRequested || (_dequeueClock.Elapsed.Subtract(startTime) >= timeout)))
                    {
                        #if false
                        _enqueueAwaiter.Notify(cancel.IsCancellationRequested ? AsyncQueueStatus.Canceled : AsyncQueueStatus.TimedOut);
                        #endif
                        item = default(T);
                        return false;
                    }
                }
            }

            // now we are sure we have at least one item in buffer, so we read it
            item = _buffer[(_tail + 1) % _capacity];

            // now it is safe to let Enqueue overwrite the dequeued slot, so we increment the _tail
            // there is no race condition in incrementing the _tail; we use Interlocked to prevent instruction reordering optimizations
            Interlocked.Increment(ref _tail);

            // handle potential race conditions when the queue was previously full, and an async enqueue operation is in progress.
            HandlePostDequeueRaceWithAsyncEnqueue(spin);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DequeueAwaiter DequeueAsync(TimeSpan timespan, CancellationToken cancel)
        {
            // calculate buffer index for the dequeue 
            int dequeueIndex = (int)((_tail + 1) % _capacity);

            // race conditions occur if the queue is empty and producer enqueues an item while this method executes
            // in order to resolve them without locking, we introduce stoplights for the producer thread
            // right after producer enqueues an item into the buffer, it checks the light at enqueue index
            // green light means that no additional action is required, and the producer is allowed to continue
            // we turn yellow light while this method is executing; producer will spin-wait until the light changes to either green or red
            // red light means that pending dequeue operation is awaiting, and the producer must signal completion to the awaiter
            _producerStoplights[dequeueIndex] = Stoplight.Yellow;

            T item;
            if (TryDequeue(out item, TimeSpan.Zero, CancellationToken.None))
            {
                // we dequeued an item - no need to await
                _producerStoplights[dequeueIndex] = Stoplight.Green;
                
                // return completed awaiter, which allows consumer continue synchronously without yielding its timeslice
                _completedDequeueAwaiter.SetDequeueCompleted(item);
                return _completedDequeueAwaiter;
            }

            // we were unable to dequeue an item right now because the queue was empty
            _pendingDequeueAwaiter.SetDequeuePending(dequeueIndex);

            // consumer task will most likely have to give up thread and await
            // unless a concurrent enqueue has just finished - we are going to find out in the next line
            // if we succeed to change yellow to red, then the buffer is still empty; but if the CompareExchange fails, then an item was just enqueued
            // this is not just a favor to consumer; we resolve a race condition which could otherwise lead to invoking awaiter continuation twice
            var mostRecentStoplight = Interlocked.CompareExchange(ref _producerStoplights[dequeueIndex], value: Stoplight.Red, comparand: Stoplight.Yellow);

            // if the light is now green, then producer has just enqueued an item,
            // and it was fast enough to change yellow to green - we've saved an await
            if (mostRecentStoplight == Stoplight.Green)
            {
                // dequeue the item
                var itemWasDequeued = TryDequeue(out item, TimeSpan.Zero, CancellationToken.None);
                Debug.Assert(itemWasDequeued);
                
                // the producer went on and won't notify our awaiter - and it's OK because there will be no await
                // we let consumer task continue synchronously
                _completedDequeueAwaiter.SetDequeueCompleted(item);
                return _completedDequeueAwaiter;
            }

            // hand off pending awaiter to sync/await state machine; 
            // next enqueue operation will notify the awaiter that there is an available item in the queue
            return _pendingDequeueAwaiter;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EnqueueAwaiter EnqueueAsync(T item, TimeSpan timespan, CancellationToken cancel)
        {
            // calculate buffer index for the dequeue 
            int enqueueIndex = (int)((_head + 1) % _capacity);

            // race conditions occur if the queue is full and consumer dequeues an item while this method executes
            // in order to resolve them without locking, we introduce stoplights for the consumer thread
            // right after consumer dequeues an item from the buffer, it checks the light at dequeue index
            // green light means that no additional action is required, and the consumer is allowed to continue
            // we turn yellow light while this method is executing; consumer will spin-wait until the light changes to either green or red
            // red light means that pending enqueue operation is awaiting, and the consumer must signal completion to the awaiter
            _consumerStoplights[enqueueIndex] = Stoplight.Yellow;

            if (TryEnqueue(item, TimeSpan.Zero, CancellationToken.None))
            {
                // we enqueued an item - no need to await
                _consumerStoplights[enqueueIndex] = Stoplight.Green;

                // return completed awaiter, which allows consumer continue synchronously without yielding its timeslice
                return _completedEnqueueAwaiter;
            }

            // we were unable to enqueue an item right now because the buffer was full
            _pendingEnqueueAwaiter.SetEnqueuePending(enqueueIndex, item);

            // producer task will most likely have to give up thread and await
            // unless a concurrent dequeue has just finished - we are going to find out in the next line
            // if we succeed to change yellow to red, then the buffer is still full; but if the CompareExchange fails, then an item was just dequeued
            // this is not just a favor to producer; we resolve a race condition which could otherwise lead to invoking awaiter continuation twice
            var mostRecentStoplight = Interlocked.CompareExchange(ref _consumerStoplights[enqueueIndex], value: Stoplight.Red, comparand: Stoplight.Yellow);

            // if the light is now green, then consumer has just dequeued an item,
            // and it was fast enough to change yellow to green - we've saved an await
            if (mostRecentStoplight == Stoplight.Green)
            {
                // enqueue the item
                var itemWasEnqueued = TryEnqueue(item, TimeSpan.Zero, CancellationToken.None);
                Debug.Assert(itemWasEnqueued);

                // the consumer went on and won't notify our awaiter - and it's OK because there will be no await
                // we let producer task continue synchronously
                return _completedEnqueueAwaiter;
            }

            // hand off pending awaiter to sync/await state machine; 
            // next dequeue operation will notify the awaiter that there is an available slot in the buffer
            return _pendingEnqueueAwaiter;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //
        // this method is called by TryDequeue running on consumer thread. 
        // this method takes steps to avoid race conditions with and notify completion of async enqueue operation, if one is pending
        //
        private void HandlePostDequeueRaceWithAsyncEnqueue(SpinWait spin)
        {
            // check the stop light. 
            var stoplightIndex = _tail % _capacity;
            var stoplight = _consumerStoplights[stoplightIndex];

            if (stoplight == Stoplight.Yellow)
            {
                // yellow means that EnqueueAsync is executing right now on producer thread
                // producer is going to CompareExchange yellow to red. if it succeeds to do so, it will give up its thread and go to await
                // if we're fast enough to CompareExchange yellow to green first, the producer task will continue synchronously without await

                stoplight = Interlocked.CompareExchange(ref _consumerStoplights[stoplightIndex], value: Stoplight.Green, comparand: Stoplight.Yellow);

                // now the stoplight is either yellow (we were faster), or one of {red, green} if the producer was faster
            }

            if (stoplight == Stoplight.Green)
            {
                // green was set by producer (or there was no simultaneous producer at our index), we're good to go
                return;
            }

            if (stoplight == Stoplight.Red)
            {
                // reset red light for future cycles
                _consumerStoplights[stoplightIndex] = Stoplight.Green;

                // enqueue the awaiting item 
                _buffer[(_head + 1) % _capacity] = _pendingEnqueueAwaiter.ItemToEnqueue;
                Interlocked.Increment(ref _head);

                // notify awaiter to let the awaiting async code continue
                _pendingEnqueueAwaiter.SetEnqueueFinished(result: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //
        // this method is called by TryEnqueue running on producer thread. 
        // this method notifies completion of awaiting async dequeue operation, if one exists
        //
        private void HandlePostEnqueueRaceWithAsyncDequeue(T item, SpinWait spin)
        {
            // check the stop light. 
            var stoplightIndex = _head % _capacity;
            var stoplight = _producerStoplights[stoplightIndex];

            if (stoplight == Stoplight.Yellow)
            {
                // yellow means that DequeueAsync is executing right now on consumer thread
                // consumer is going to CompareExchange yellow to red. if it succeeds to do so, it will give up its thread and go to await
                // if we're fast enough to CompareExchange yellow to green first, the consumer task will continue synchronously without await

                stoplight = Interlocked.CompareExchange(ref _producerStoplights[stoplightIndex], value: Stoplight.Green, comparand: Stoplight.Yellow);

                // now the stoplight is either yellow (we were faster), or one of {green, red} if the consumer was faster
            }

            if (stoplight == Stoplight.Green)
            {
                // green was set by consumer (or there was no simultaneous consumer at our index), we're good to go
                return;
            }

            if (stoplight == Stoplight.Red)
            {
                // reset red light for future cycles
                _producerStoplights[stoplightIndex] = Stoplight.Green;

                // dequeue the newly enqueued item
                Interlocked.Increment(ref _tail);

                // notify awaiter to let the awaiting async code continue
                _pendingDequeueAwaiter.SetDequeueFinished(AsyncQueueStatus.Completed, item);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DebuggerDisplay("{_name}")]
        private class Stoplight
        {
            private readonly string _name;

            private Stoplight(string name)
            {
                _name = name;
            }

            public override string ToString()
            {
                return _name;
            }

            public static readonly Stoplight Green = new Stoplight("Green");
            public static readonly Stoplight Yellow = new Stoplight("Yellow");
            public static readonly Stoplight Red = new Stoplight("Red");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class EnqueueAwaiter : INotifyCompletion
        {
            public EnqueueAwaiter GetAwaiter()
            {
                return this;
            }

            public abstract bool IsCompleted { get; }
            public abstract bool GetResult();
            public abstract void OnCompleted(Action continuation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CompletedEnqueueAwaiter : EnqueueAwaiter
        {
            #region Overrides of DequeueAwaiter

            public override bool IsCompleted
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool GetResult()
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void OnCompleted(Action continuation)
            {
                throw new InvalidOperationException("The operation completed synchronously.");
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class PendingEnqueueAwaiter : EnqueueAwaiter
        {
            private readonly SingleProducerConsumerQueue<T> _owner;
            private readonly WaitCallback _onExecuteContinuationCallback;
            private volatile Action _continuation;
            private T _itemToEnqueue;
            private bool? _result;
            private int _enqueueIndex;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PendingEnqueueAwaiter(SingleProducerConsumerQueue<T> owner)
            {
                _owner = owner;
                _onExecuteContinuationCallback = new WaitCallback(this.OnExecuteContinuation);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 1.
            // EnqueueAsync calls us when it is unable to enqueue an item into an available slot (the buffer is full).
            // it reuses our single instance and wants to prepare our state prior to returning us to async/await state machine
            //
            public void SetEnqueuePending(int enqueueIndex, T itemToEnqueue)
            {
                // this call is assumed to be (almost) immediately followed by a call to OnCompleted
                // because this is how compiler-generated async/await state machine behaves

                _result = null;
                _itemToEnqueue = itemToEnqueue;
                _enqueueIndex = enqueueIndex;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEPS 2 & 5. 
            // Async/await state machine calls us to check whether the operation completed.
            // On step 2, the state machine checks whether the operation completed synchronously - we return false.
            // Step 5 happens upon resuming after await - we return true.
            //
            public override bool IsCompleted
            {
                get
                {
                    return _result.HasValue;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 3.
            // Async/await state machine sets a delegate, which needs to be invoked in order to resume execution after await.
            //
            public override void OnCompleted(Action continuation)
            {
                // this is the action we are required to run as soon as the operation completes
                _continuation = continuation;

                // switch to red light - the consumer will know that it has to call SetEnqueueFinished
                _owner._consumerStoplights[_enqueueIndex] = Stoplight.Red;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 4.
            // When TryDequeue successfully completes, it calls us to notify that a free slot in the buffer is available.
            // In response, we update our internal state, and queue continuation delegate for execution on ThreadPool.
            // Here we have a race condition with OnCompleted, which might has not been called yet, and the _continuation delegate was not assigned.
            // This happens when the buffer is full, and TryDequeue works at around the same time with EnqueueAsync.
            // To resolve, we spin-wait until _continuation is assigned.
            //
            public void SetEnqueueFinished(bool result)
            {
                var spin = new SpinWait();

                // make sure _continuation delegate was assigned
                while (_continuation == null)
                {
                    // waiting until OnCompleted is invoked by async/await state machine, which should happen almost immediattely
                    spin.SpinOnce();
                }

                // now that continuation was assigned, it is safe to update results and queue the continuation on ThreadPool.
                _result = result;
                ThreadPool.UnsafeQueueUserWorkItem(_onExecuteContinuationCallback, _continuation);

                // we've done - cleanup the _continuation to let GC collect it after it finishes
                _continuation = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 6.
            // Async/await state machine retrieves the result of the operation, upon resuming after await.
            //
            public override bool GetResult()
            {
                // ReSharper disable once PossibleInvalidOperationException
                // since STEP 6 happens after operation results are assigned, we know that _result has value.
                return _result.Value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T ItemToEnqueue
            {
                get { return _itemToEnqueue; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnExecuteContinuation(object continuationAction)
            {
                ((Action)continuationAction)();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public abstract class DequeueAwaiter : INotifyCompletion
        {
            public DequeueAwaiter GetAwaiter()
            {
                return this;
            }

            public abstract bool IsCompleted { get; }
            public abstract DequeueResult<T> GetResult();
            public abstract void OnCompleted(Action continuation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class CompletedDequeueAwaiter : DequeueAwaiter
        {
            private DequeueResult<T> _result;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of DequeueAwaiter

            public override bool IsCompleted
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override DequeueResult<T> GetResult()
            {
                return _result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void OnCompleted(Action continuation)
            {
                throw new InvalidOperationException("The operation completed synchronously.");
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetDequeueCompleted(T dequeuedItem)
            {
                _result.Status = AsyncQueueStatus.Completed;
                _result.Item = dequeuedItem;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class PendingDequeueAwaiter : DequeueAwaiter
        {
            private readonly SingleProducerConsumerQueue<T> _owner;
            private readonly WaitCallback _onExecuteContinuationCallback;
            private volatile Action _continuation;
            private DequeueResult<T> _result;
            private int _dequeueIndex;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PendingDequeueAwaiter(SingleProducerConsumerQueue<T> owner)
            {
                _owner = owner;
                _onExecuteContinuationCallback = new WaitCallback(this.OnExecuteContinuation);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
                _result.Status = AsyncQueueStatus.Awaiting;
                _result.Item = default(T);
                _continuation = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 1.
            // DequeueAsync calls us when it is unable to dequeue an available item.
            // it reuses our single instance and wants to prepare our state prior to returning us to async/await state machine
            //
            public void SetDequeuePending(int dequeueIndex)
            {
                // this call is assumed to be (almost) immediately followed by a call to OnCompleted
                // because this is how compiler-generated async/await state machine behaves

                _result.Status = AsyncQueueStatus.Awaiting;
                _result.Item = default(T);
                _dequeueIndex = dequeueIndex;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEPS 2 & 5. 
            // Async/await state machine calls us to check whether the result is available.
            // On step 2, the state machine checks whether the operation completed synchronously - we return false.
            // Step 5 happens upon resuming after await - we return true.
            //
            public override bool IsCompleted
            {
                get
                {
                    return (_result.Status != AsyncQueueStatus.Awaiting);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 3.
            // Async/await state machine sets a delegate, which needs to be invoked in order to resume execution after await.
            //
            public override void OnCompleted(Action continuation)
            {
                // this is the action we need to run as soon as the operation is completed
                _continuation = continuation;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 4.
            // When TryEnqueue successfully completes, it calls us to notify that an item was enqueued.
            // In response, we update our internal state, and queue continuation delegate for execution on ThreadPool.
            // Here we have a race condition with OnCompleted, which might has not been called yet, and the _continuation delegate was not assigned.
            // This happens when the queue is empty, and TryEnqueue works at around the same time with DequeueAsync.
            // To resolve, we spin-wait until _continuation is assigned.
            //
            public void SetDequeueFinished(AsyncQueueStatus status, T item)
            {
                var spin = new SpinWait();

                // make sure _continuation delegate was assigned
                while (_continuation == null)
                {
                    // waiting until OnCompleted is invoked by async/await state machine, which should happen almost immediattely
                    spin.SpinOnce();
                }

                // now that continuation was assigned, it is safe to update results and queue the continuation on ThreadPool.
                _result.Status = status;
                _result.Item = item;
                ThreadPool.UnsafeQueueUserWorkItem(_onExecuteContinuationCallback, _continuation);

                // we've done - cleanup the _continuation to let GC collect it after it finishes
                _continuation = null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            //
            // STEP 6.
            // Async/await state machine retrieves the result of the operation, upon resuming after await.
            //
            public override DequeueResult<T> GetResult()
            {
                return _result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnExecuteContinuation(object continuationAction)
            {
                ((Action)continuationAction)();
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum AsyncQueueStatus
    {
        Awaiting,
        Completed,
        TimedOut,
        Canceled
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public struct DequeueResult<T>
    {
        public AsyncQueueStatus Status;
        public T Item;
    }
}
