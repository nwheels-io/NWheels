using System;
using System.Diagnostics;
using System.Threading;

namespace NWheels.Concurrency
{
    //
    // Single producer/single consumer lock-free FIFO queue, based on ring buffer and spin-waits.
    //
    internal class SingleProducerConsumerQueue<T>
    {
        // lenth of the ring buffer
        private readonly long _capacity;

        // the ring buffer
        private readonly T[] _buffer;

        // tracks enqueue operation timeout; 
        // one instance is reused by all Enqueue operations
        private readonly Stopwatch _enqueueClock;

        // tracks dequeue operation timeout; 
        // one instance is reused by all Dequeue operations
        private readonly Stopwatch _dequeueClock;

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
        public SingleProducerConsumerQueue(long capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
            _enqueueClock = Stopwatch.StartNew();
            _dequeueClock = Stopwatch.StartNew();
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
            // check if we have space in buffer for another item
            if (_head - _tail >= _capacity)
            {
                // the buffer is full. we have to wait until the consumer dequeues at least one item
                // use combination of spin-wait/yield (implemented by SpinWait struct) until at least one slot becomes available

                var startTime = _enqueueClock.Elapsed;
                var spin = new SpinWait();
                var spinCount = 0;

                while (_head - _tail >= _capacity)
                {
                    spinCount++;
                    spin.SpinOnce(); // each time SpinOnce will decide to either spin or yield

                    // timeout/cancellation check costs some CPU cycles
                    // because of that, we check for timeout/cancellation once every 100 iterations
                    if ((spinCount % 100) == 0 && (cancel.IsCancellationRequested || (_dequeueClock.Elapsed.Subtract(startTime) >= timeout)))
                    {
                        return false;
                    }
                }
            }

            // now we are sure we have space for the new item
            _buffer[(_head + 1) % _capacity] = item;

            // now it is safe to let Dequeue read the enqueued slot, so we increment the _head
            // there is no race condition in incrementing the _head; we use Interlocked to prevent instruction reordering optimizations
            Interlocked.Increment(ref _head);

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
    }
}