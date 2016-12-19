using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency.Core
{
    public class TaskBasedDeferred : TaskCompletionSource<bool>, IDeferred, IAnyDeferred
    {
        private Action _continuation = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TaskBasedDeferred()
        {
            base.Task.ConfigureAwait(continueOnCapturedContext: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Resolve()
        {
            base.SetResult(true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAnyDeferred.Resolve(object result)
        {
            base.SetResult(true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            base.SetException(error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyDeferred.ResultType
        {
            get { return null; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyDeferred.Result
        {
            get
            {
                if (base.Task.Exception != null)
                {
                    throw base.Task.Exception;
                }
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(Action continuation = null, TimeSpan? timeout = null, System.Threading.CancellationToken? cancellation = null)
        {
            if (continuation != null)
            {
                _continuation = continuation;
                base.Task.ContinueWith(_s_onScheduleContinuationDelegate, this, cancellation.GetValueOrDefault(CancellationToken.None));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get { return base.Task.IsCompleted; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Exception Error
        {
            get { return base.Task.Exception; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Action<Task<bool>, object> _s_onScheduleContinuationDelegate = OnScheduleContinuation;
        private static readonly WaitCallback _s_onRunContinuationDelegate = OnRunContinuation;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void OnScheduleContinuation(Task<bool> task, object deferredObject)
        {
            var deferred = (TaskBasedDeferred)deferredObject;
            ThreadPool.UnsafeQueueUserWorkItem(_s_onRunContinuationDelegate, deferred._continuation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void OnRunContinuation(object continuationAction)
        {
            var action = (Action)continuationAction;
            action();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TaskBasedDeferred<T> : TaskCompletionSource<T>, IDeferred<T>, IAnyDeferred
    {
        private Action _continuation = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TaskBasedDeferred()
        {
            base.Task.ConfigureAwait(continueOnCapturedContext: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Resolve(T result)
        {
            base.SetResult(result);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAnyDeferred.Resolve(object result)
        {
            var resultAsTask = (result as Task<T>);

            if (resultAsTask != null)
            {
                base.SetResult(resultAsTask.Result);
            }
            else
            {
                base.SetResult((T)result);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            base.SetException(error);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyDeferred.ResultType
        {
            get
            {
                return typeof(T);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyDeferred.Result
        {
            get
            {
                return base.Task.Result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(
            Action continuation = null,
            TimeSpan? timeout = null,
            System.Threading.CancellationToken? cancellation = null)
        {
            if (continuation != null)
            {
                _continuation = continuation;
                base.Task.ContinueWith(_s_onScheduleContinuationDelegate, this, cancellation.GetValueOrDefault(CancellationToken.None));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get { return base.Task.IsCompleted; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Result
        {
            get { return base.Task.Result; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception Error
        {
            get { return base.Task.Exception; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Action<Task<T>, object> _s_onScheduleContinuationDelegate = OnScheduleContinuation;
        private static readonly WaitCallback _s_onRunContinuationDelegate = OnRunContinuation;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void OnScheduleContinuation(Task<T> task, object deferredObject)
        {
            var deferred = (TaskBasedDeferred<T>)deferredObject;
            ThreadPool.UnsafeQueueUserWorkItem(_s_onRunContinuationDelegate, deferred._continuation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void OnRunContinuation(object continuationAction)
        {
            var action = (Action)continuationAction;
            action();
        }
    }
}
