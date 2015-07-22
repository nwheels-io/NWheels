using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace NWheels.Stacks.Network
{
    public class MessagesDispatcher
    {
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        public class MessagesHandlingPerformanceInfo
        {
            public int NumOfHandledMessages { get; set; }
            public long MinMessageWaitingTime { get; set; }
            public long MaxMessageWaitingTime { get; set; }
            public long AvgMessageWaitingTime { get; set; }
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////

        private string _threadName;
        private bool _continueRunning;
        private readonly ManualResetEvent _newMsgEvt = new ManualResetEvent(false);
        private readonly Queue<QueueMessageData> _messagesQueue = new Queue<QueueMessageData>();

        private int _activeThreadsCounter;
        private MessageDispatcherEndedDlgt _messageDispatcherEndedDlgt;
        private object _endedDlgtParam;

        //===================================
        private Stopwatch _stopWatch;
        private int _numOfHandledMessages;
        private long _summaryOfMessagesWaitingTimes;
        private long _minMessageWaitingTime;
        private long _maxMessageWaitingTime;
        //===================================

        public delegate void ExecutionDlgt(object o);
        public delegate void MessageDispatcherEndedDlgt(object cbParam);
        public bool IsRunning { get { return _continueRunning; } }

        public MessagesDispatcher() :
            this(String.Empty)
        {
        }

        public MessagesDispatcher(string threadName)
        {
            MaxSecondsPerDelegate = 2;
            _threadName = threadName;
        }

        // When a delegate returns after this amount of time, an error is printed.
        public int MaxSecondsPerDelegate { get; set; }

        // stopWatch - an instance to be used for measuring the time.
        // Set to null to disable the measurments.
        public void EnableMeasureTiming(Stopwatch stopWatch)
        {
            lock (this)
            {
                _stopWatch = stopWatch;
                ResetPerformanceMeter();
            }
        }

        public MessagesHandlingPerformanceInfo GetMeasureResultsAndReset()
        {
            MessagesHandlingPerformanceInfo result = new MessagesHandlingPerformanceInfo();
            lock (this)
            {
                result.NumOfHandledMessages = _numOfHandledMessages;
                result.MinMessageWaitingTime = _minMessageWaitingTime;
                result.MaxMessageWaitingTime = _maxMessageWaitingTime;
                if (_numOfHandledMessages > 0)
                {
                    result.AvgMessageWaitingTime = _summaryOfMessagesWaitingTimes / _numOfHandledMessages;
                }

                ResetPerformanceMeter();
            }

            return result;
        }

        private void ResetPerformanceMeter()
        {
            _numOfHandledMessages = 0;
            _summaryOfMessagesWaitingTimes = 0;
            _minMessageWaitingTime = long.MaxValue;
            _maxMessageWaitingTime = 0;
        }

        public void RegisterMessageDispatcherEndedDlgt(MessageDispatcherEndedDlgt dlgt, object cbParam)
        {
            _messageDispatcherEndedDlgt = dlgt;
            _endedDlgtParam = cbParam;
        }

        // Start running - activate one thread to handle the messages.
        public void Run()
        {
            _continueRunning = true;
            _activeThreadsCounter = 1;
            Thread tr = new Thread(ThreadHandler);
            if (!String.IsNullOrEmpty(_threadName))
                tr.Name = _threadName;
            tr.Start();
        }

        // Start running - activate one or more threads to handle the messages.
        // Use this method in case you want to acelerate the handling of your code and you can handle multithreading work.
        public void Run(int numOfParallelThreads)
        {
            _continueRunning = true;
            _activeThreadsCounter = numOfParallelThreads;
            for (int i = 0; i < numOfParallelThreads; ++i)
            {
                Thread tr = new Thread(ThreadHandler);
                if (!String.IsNullOrEmpty(_threadName))
                    tr.Name = _threadName + "_" + i;
                tr.Start();
            }
        }

        public void Stop(bool isWithCleanQueue)
        {
            lock (_messagesQueue)
            {
                _continueRunning = false;
                if (isWithCleanQueue)
                    _messagesQueue.Clear();
                _newMsgEvt.Set();
            }
        }

        public void EnqueueTask(ExecutionDlgt dlgt, object obj)
        {
            QueueMessageData m = new QueueMessageData(dlgt, obj);
            if (_stopWatch != null)
                m.CreationTime = _stopWatch.ElapsedMilliseconds;
            lock (_messagesQueue)
            {
                if (!_continueRunning)
                    return;
                _messagesQueue.Enqueue(m);
                _newMsgEvt.Set();
            }
        }

        private class QueueMessageData
        {
            public QueueMessageData(ExecutionDlgt dlgt, object obj)
            {
                ExecutionDelegate = dlgt;
                DelegateParameter = obj;
            }

            public long CreationTime;
            public readonly ExecutionDlgt ExecutionDelegate;
            public readonly object DelegateParameter;
        }

        private void ThreadHandler()
        {
            do
            {
                _newMsgEvt.WaitOne();
                if (_continueRunning)
                {
                    while (_messagesQueue.Count > 0)
                    {
                        try
                        {
                            QueueMessageData QueueMessageData = null;
                            lock (_messagesQueue)
                            {
                                if (_messagesQueue.Count != 0)
                                {
                                    QueueMessageData = _messagesQueue.Dequeue();
                                }
                                if (_messagesQueue.Count == 0)
                                    _newMsgEvt.Reset();
                            }

                            if (QueueMessageData != null)
                            {
                                DateTime timeBeforeStartHandle = DateTime.UtcNow;
                                if (_stopWatch != null && QueueMessageData.CreationTime != 0)
                                {
                                    long waitingTime = _stopWatch.ElapsedMilliseconds - QueueMessageData.CreationTime;
                                    lock (this)
                                    {
                                        _numOfHandledMessages++;
                                        _summaryOfMessagesWaitingTimes += waitingTime;
                                        if (_minMessageWaitingTime > waitingTime)
                                            _minMessageWaitingTime = waitingTime;
                                        if (_maxMessageWaitingTime < waitingTime)
                                            _maxMessageWaitingTime = waitingTime;
                                    }
                                    if (waitingTime >= MaxSecondsPerDelegate * 1000)
                                    {
                                        // Todo: Eli - log / change perfmeter method
                                        //LogUtils.LogError(
                                        //    LogUtils.DefaultLogNamespace,
                                        //    "Dispatcher {0}\n Encountered a too long waiting in queue: {1} seconds.",
                                        //    m_ThreadName,
                                        //    waitingTime / 1000.0);
                                    }
                                }

                                Delegate invokedDelegate = null;
                                if (QueueMessageData.ExecutionDelegate != null)
                                {
                                    QueueMessageData.ExecutionDelegate(QueueMessageData.DelegateParameter);
                                    invokedDelegate = QueueMessageData.ExecutionDelegate;
                                }

                                TimeSpan handlingTime = DateTime.UtcNow.Subtract(timeBeforeStartHandle);
                                if (handlingTime.TotalSeconds >= MaxSecondsPerDelegate && invokedDelegate != null)
                                {
                                    // Todo: Eli - log / change perfmeter method
                                    //LogUtils.LogError(
                                    //    LogUtils.DefaultLogNamespace,
                                    //    "Dispatcher {0}\n Encountered a too long processing of {1} message : {2} seconds.",
                                    //    m_ThreadName,
                                    //    invokedDelegate.Method.Name,
                                    //    handlingTime.TotalSeconds);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Todo: Eli - log error
                            Console.WriteLine(ex.Message);
                            //LogUtils.LogError(LogUtils.DefaultLogNamespace, "Dispatcher {0} Exception:\n{1}",
                            //    m_ThreadName,
                            //    ex.ToString());
                        }
                    }
                }
            } while (_continueRunning);

            ThreadEnded();
        }

        private void ThreadEnded()
        {
            bool callEndCb = false;

            lock (_messagesQueue)
            {
                _activeThreadsCounter--;
                callEndCb = (_activeThreadsCounter == 0);
            }

            if (callEndCb && _messageDispatcherEndedDlgt != null)
            {
                _messageDispatcherEndedDlgt(_endedDlgtParam);
            }
        }
    }
}
