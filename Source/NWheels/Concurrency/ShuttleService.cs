using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing.Messages;

namespace NWheels.Concurrency
{
    public class ShuttleService<T>
    {
        private readonly object _syncRoot = new object();
        private readonly IFramework _framework;
        private readonly string _serviceName;
        private readonly int _maxItemsOnBoard;
        private readonly TimeSpan _boardingTimeout;
        private readonly int _driverThreadCount;
        private readonly Action<T[]> _driver;
        private readonly IShuttleServiceLogger _logger;
        private CancellationTokenSource _cancellation;
        private BlockingCollection<T[]> _departureQueue;
        private Thread[] _driverThreads;
        private T[] _itemsOnBoard;
        private int _numItemsOnBoard;
        private DateTime _departureAtUtc;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ShuttleService(
            IFramework framework, 
            string serviceName, 
            int maxItemsOnBoard, 
            TimeSpan boardingTimeout, 
            int driverThreadCount, 
            Action<T[]> driver)
        {
            _serviceName = serviceName;
            _driverThreadCount = driverThreadCount;
            _framework = framework;
            _maxItemsOnBoard = maxItemsOnBoard;
            _boardingTimeout = boardingTimeout;
            _driver = driver;
            
            _itemsOnBoard = null;
            _numItemsOnBoard = 0;

            _logger = _framework.As<ICoreFramework>().Components.Resolve<IShuttleServiceLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start()
        {
            _cancellation = new CancellationTokenSource();
            _departureQueue = new BlockingCollection<T[]>();
            _driverThreads = new Thread[_driverThreadCount];
            _itemsOnBoard = new T[_maxItemsOnBoard];

            Flush();
            StartDriverThreads();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Stop(TimeSpan timeout)
        {
            if ( !_cancellation.IsCancellationRequested )
            {
                _cancellation.Cancel();
            }

            try
            {
                bool anyThreadDidNotStopInTimelyFashion;
                StopDriverThreads(timeout, out anyThreadDidNotStopInTimelyFashion);

                if ( anyThreadDidNotStopInTimelyFashion )
                {
                    _logger.ShuttleServiceDidNotStopProperly(_serviceName);
                }
                else
                {
                    _logger.ShuttleServiceSuccessfullyStopped(_serviceName);
                }

                FlushRemainingItems();
            }
            catch ( Exception e )
            {
                _logger.ShuttleServiceHasFailedToStop(_serviceName, e);
            }

            _cancellation.Dispose();
            _departureQueue.Dispose();

            _cancellation = null;
            _departureQueue = null;
            _driverThreads = null;
            _itemsOnBoard = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FlushRemainingItems()
        {
            using ( var activity = _logger.FlushingRemainingItems(_serviceName) )
            {
                Flush();

                T[] batch;

                while ( _departureQueue.TryTake(out batch) )
                {
                    if ( !InvokeDriver(-1, batch) )
                    {
                        _logger.FailedToFlushLastItems(_serviceName);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Board(T item)
        {
            if ( _cancellation.IsCancellationRequested )
            {
                return false;
            }

            lock ( _syncRoot )
            {
                _itemsOnBoard[_numItemsOnBoard] = item;
                _numItemsOnBoard++;

                if ( _numItemsOnBoard >= _maxItemsOnBoard )
                {
                    Flush();
                }
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Flush()
        {
            lock ( _syncRoot )
            {
                if ( _numItemsOnBoard > 0 )
                {
                    var batch = new T[_numItemsOnBoard];
                    
                    Array.Copy(_itemsOnBoard, batch, _numItemsOnBoard);
                    _departureQueue.Add(batch);

                    for ( int i = 0 ; i < _numItemsOnBoard ; i++ )
                    {
                        _itemsOnBoard[i] = default(T);
                    }
                    
                    _numItemsOnBoard = 0;
                }

                _departureAtUtc = _framework.UtcNow.Add(_boardingTimeout);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StartDriverThreads()
        {
            for ( int i = 0; i < _driverThreads.Length; i++ )
            {
                var threadIndex = i;

                _driverThreads[i] = _framework.As<ICoreFramework>().CreateThread(
                    () => RunDriverThread(threadIndex),
                    taskType: ThreadTaskType.BatchWorker,
                    description: string.Format("{0}[{1}/{2}]", _serviceName, threadIndex, _driverThreadCount));
            }

            for ( int i = 0; i < _driverThreads.Length; i++ )
            {
                _driverThreads[i].Start();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StopDriverThreads(TimeSpan timeout, out bool anyThreadDidNotStopInTimelyFashion)
        {
            anyThreadDidNotStopInTimelyFashion = false;
            var stopStartedAtUtc = _framework.UtcNow;

            for ( int i = 0; i < _driverThreads.Length; i++ )
            {
                var elapsedTime = _framework.UtcNow.Subtract(stopStartedAtUtc);
                var remainingTimeout = (elapsedTime > timeout ? TimeSpan.Zero : timeout.Subtract(elapsedTime));

                if ( !_driverThreads[i].Join(remainingTimeout) )
                {
                    anyThreadDidNotStopInTimelyFashion = true;
                    _logger.DriverThreadDidNotStopInTimelyFashion(_serviceName, threadIndex: i, managedThreadId: _driverThreads[i].ManagedThreadId);

                    _driverThreads[i].Abort();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunDriverThread(int threadIndex)
        {
            _logger.DriverThreadStarted(_serviceName, threadIndex, Thread.CurrentThread.ManagedThreadId);

            while ( !_cancellation.IsCancellationRequested )
            {
                T[] batch;

                try
                {
                    var timeToDeparture = GetTimeToDeparture();

                    if ( _departureQueue.TryTake(out batch, (int)timeToDeparture.TotalMilliseconds, _cancellation.Token) )
                    {
                        InvokeDriver(threadIndex, batch);
                    }
                    else
                    {
                        Flush();
                        
                        if ( _departureQueue.TryTake(out batch) )
                        {
                            InvokeDriver(threadIndex, batch);
                        }
                    }
                }
                catch ( OperationCanceledException )
                {
                    _logger.DriverThreadShutdownRequested(_serviceName, threadIndex, Thread.CurrentThread.ManagedThreadId);
                }
            }

            _logger.DriverThreadStopped(_serviceName, threadIndex, Thread.CurrentThread.ManagedThreadId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool InvokeDriver(int threadIndex, T[] batch)
        {
            using ( var activity = _logger.InvokingDriver(_serviceName, threadIndex, batch.Length) )
            {
                try
                {
                    _driver(batch);
                    return true;
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.DriverInvocationFailed(_serviceName, threadIndex, batch.Length, e);
                    return false;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TimeSpan GetTimeToDeparture()
        {
            DateTime departureTimeSnapshot;

            lock ( _syncRoot )
            {
                departureTimeSnapshot = _departureAtUtc;
            }

            var timeout = departureTimeSnapshot.Subtract(_framework.UtcNow);

            if ( timeout > TimeSpan.Zero )
            {
                return timeout;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IShuttleServiceLogger : IApplicationEventLogger
    {
        [LogVerbose]
        void DriverThreadStarted(string serviceName, int threadIndex, int managedThreadId);

        [LogInfo]
        void ShuttleServiceSuccessfullyStarted(string serviceName);

        [LogVerbose]
        void DriverThreadShutdownRequested(string serviceName, int threadIndex, int managedThreadId);
        
        [LogVerbose]
        void DriverThreadStopped(string serviceName, int threadIndex, int managedThreadId);

        [LogCritical]
        void DriverThreadDidNotStopInTimelyFashion(string serviceName, int threadIndex, int managedThreadId);

        [LogActivity]
        ILogActivity FlushingRemainingItems(string serviceName);

        [LogCritical]
        void FailedToFlushLastItems(string serviceName);

        [LogInfo]
        void ShuttleServiceSuccessfullyStopped(string serviceName);

        [LogWarning]
        void ShuttleServiceDidNotStopProperly(string serviceName);

        [LogCritical]
        void ShuttleServiceHasFailedToStop(string serviceName, Exception error);

        [LogActivity]
        ILogActivity InvokingDriver(string serviceName, int threadIndex, int itemsInBatch);

        [LogError]
        void DriverInvocationFailed(string serviceName, int threadIndex, int itemsInBatch, Exception error);
    }
}
