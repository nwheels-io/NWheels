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

        // tracks enqueue operation timeout; 
        // one instance is reused by all Enqueue operations
        private readonly Stopwatch _enqueueClock;

        // tracks dequeue operation timeout; 
        // one instance is reused by all Dequeue operations
        private readonly Stopwatch _dequeueClock;

        //
        #if false
        private readonly EnqueueAwaiter _enqueueAwaiter;
        #endif

        // 
        private readonly CompletedDequeueAwaiter _completedDequeueAwaiter;

        // 
        private readonly PendingDequeueAwaiter _pendingDequeueAwaiter;

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
            _enqueueClock = Stopwatch.StartNew();
            _dequeueClock = Stopwatch.StartNew();
            
            //_enqueueAwaiter = new EnqueueAwaiter(this);
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

            // check the stop light. 
            // if the queue was empty and DequeueAsync is currently executing on consumer thread, the light will be yellow
            // to avoid race conditions, we wait until the light changes to either green of red (which should happen very fast)
            var stoplightIndex = _head % _capacity;
            
            while (_producerStoplights[stoplightIndex] == Stoplight.Yellow)
            {
                spin.SpinOnce();
            }

            // red light means there is an awaiting async dequeue operation.
            if (_producerStoplights[stoplightIndex] == Stoplight.Red)
            {
                // reset red light for future cycles
                _producerStoplights[stoplightIndex] = Stoplight.Green;
                
                // dequeue the item back
                Interlocked.Decrement(ref _head);
                
                // notify awaiter to let the async code continue
                _pendingDequeueAwaiter.SetDequeueFinished(AsyncQueueStatus.Completed, item);
            }

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
            // check if we have at least one item in the queue
            if (_tail >= _head)
            {
                // the buffer is empty. we have to wait until the producer enqueues at least one item
                // use combination of spin-wait/yield (implemented by SpinWait struct) until at least one item  becomes available

                var startTime = _dequeueClock.Elapsed;
                var spin = new SpinWait();
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

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false
        public EnqueueAwaiter EnqueueAsync(T item, TimeSpan timeout, CancellationToken canel)
        {
            // check if we have space in buffer for another item
            if (_head - _tail < _capacity)
            {
                // we have space for the new item, write it
                _buffer[(_head + 1) % _capacity] = item;

                // now it is safe to let Dequeue read the enqueued slot, so we increment the _head
                // there is no race condition in incrementing the _head; we use Interlocked to prevent instruction reordering optimizations
                Interlocked.Increment(ref _head);

                // setting our awaiter to completed state. this will allow calling async method to continue synchronously
                //_enqueueAwaiter.Notify(AsyncQueueStatus.Completed);
            }
            else
            {
                // the buffer is full. the caller has to wait until the consumer dequeues at least one item
                //_enqueueAwaiter.Reset();
            }
            
            return _enqueueAwaiter;
        }
        #endif

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

            // we were unable to dequeue an item right now - consumer thread will have to yield and await
            _pendingDequeueAwaiter.SetDequeuePending(dequeueIndex);
            return _pendingDequeueAwaiter;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum Stoplight
        {
            Green,
            Yellow,
            Red
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false
        public class EnqueueAwaiter : INotifyCompletion
        {
            private readonly SingleProducerConsumerQueue<T> _owner;
            private AsyncQueueStatus _result;
            private Action _continuation;

            public EnqueueAwaiter(SingleProducerConsumerQueue<T> owner)
            {
                _owner = owner;
            }

            public void Reset()
            {
                _result = AsyncQueueStatus.Awaiting;
                _continuation = null;
            }

            public void Notify(AsyncQueueStatus result)
            {
                _result = result;

                if (_continuation != null)
                {
                    Task.Factory.StartNew(_continuation);
                }
            }

            public EnqueueAwaiter GetAwaiter()
            {
                return this;
            }

            #region Implementation of INotifyCompletion

            public void OnCompleted(Action continuation)
            {
                _continuation = continuation;
            }

            #endregion

            public AsyncQueueStatus GetResult()
            {
                return _result;
            }

            public bool IsCompleted
            {
                get
                {
                    return (_result != AsyncQueueStatus.Awaiting);
                }
            }
        }
        #endif

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
            private DequeueResult<T> _result;
            private int _dequeueIndex;
            private Action _continuation;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PendingDequeueAwaiter(SingleProducerConsumerQueue<T> owner)
            {
                _owner = owner;
                _onExecuteContinuationCallback = new WaitCallback(this.OnExecuteContinuation);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            #region Overrides of DequeueAwaiter

            public override bool IsCompleted
            {
                get
                {
                    return (_result.Status != AsyncQueueStatus.Awaiting);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public override DequeueResult<T> GetResult()
            {
                return _result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public override void OnCompleted(Action continuation)
            {
                // this is the action we need to run as soon as the operation is completed
                _continuation = continuation;
                
                // switch to red light - the producer will know that it has to call SetDequeueFinished
                _owner._producerStoplights[_dequeueIndex] = Stoplight.Red;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetDequeuePending(int dequeueIndex)
            {
                // this call is assumed to be immediately followed by a call to OnCompleted

                _result.Status = AsyncQueueStatus.Awaiting;
                _result.Item = default(T);
                _dequeueIndex = dequeueIndex;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetDequeueFinished(AsyncQueueStatus status, T item)
            {
                _result.Status = status;
                _result.Item = item;
                _owner._producerStoplights[_dequeueIndex] = Stoplight.Green;
                
                ThreadPool.UnsafeQueueUserWorkItem(_onExecuteContinuationCallback, _continuation);
                _continuation = null;
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
