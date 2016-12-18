using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency.Core
{
    public abstract class DeferredBase
    {
        private int _locked;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void Lock()
        {
            var wait = new SpinWait();

            while (Interlocked.CompareExchange(ref _locked, 1, 0) != 0)
            {
                wait.SpinOnce();
            }    
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void Unlock()
        {
            var previouslyLocked = Interlocked.CompareExchange(ref _locked, 0, 1);
            Debug.Assert(previouslyLocked == 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly WaitCallback _s_executeContinuationCallback = ExecuteContinuationCallback;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected static void ScheduleContinuation(Action continuation)
        {
            ThreadPool.UnsafeQueueUserWorkItem(_s_executeContinuationCallback, state: continuation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ExecuteContinuationCallback(object continuationAction)
        {
            Action continuation = (Action)continuationAction;
            continuation();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Deferred : DeferredBase, IDeferred, IAnyDeferred
    {
        private volatile Action _continuation = null;
        private volatile bool _isResolved = false;
        private Exception _error = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Resolve()
        {
            base.Lock();

            try
            {
                _isResolved = true;
                _error = null;

                if (_continuation != null)
                {
                    ScheduleContinuation(_continuation);
                }
            }
            finally
            {
                base.Unlock();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAnyDeferred.Resolve(object result)
        {
            this.Resolve();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            base.Lock();

            try
            {
                _isResolved = true;
                _error = error;

                if (_continuation != null)
                {
                    ScheduleContinuation(_continuation);
                }
            }
            finally
            {
                base.Unlock();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(Action continuation, TimeSpan? timeout, System.Threading.CancellationToken? cancellation)
        {
            base.Lock();

            try
            {
                if (continuation != null)
                {
                    _continuation = continuation;
                }

                if (_continuation != null && _isResolved)
                {
                    ScheduleContinuation(_continuation);
                }
            }
            finally
            {
                base.Unlock();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get
            {
                return _isResolved;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyDeferred.Result
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyDeferred.ResultType
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception Error
        {
            get
            {
                if (!_isResolved)
                {
                    throw new InvalidOperationException("Cannot retrieve error because operation is still in progress.");
                }

                return _error;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Deferred<T> : DeferredBase, IDeferred<T>, IAnyDeferred
    {
        private volatile Action _continuation = null;
        private volatile bool _isResolved = false;
        private T _result = default(T);
        private Exception _error = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Resolve(T result)
        {
            base.Lock();

            try
            {
                _isResolved = true;
                _result = result;
                _error = null;

                if (_continuation != null)
                {
                    ScheduleContinuation(_continuation);
                }
            }
            finally
            {
                base.Unlock();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAnyDeferred.Resolve(object result)
        {
            this.Resolve((T)result);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            base.Lock();

            try
            {
                _isResolved = true;
                _result = default(T);
                _error = error;

                if (_continuation != null)
                {
                    ScheduleContinuation(_continuation);
                }
            }
            finally
            {
                base.Unlock();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(Action continuation, TimeSpan? timeout, System.Threading.CancellationToken? cancellation)
        {
            base.Lock();

            try
            {
                if (continuation != null)
                {
                    _continuation = continuation;
                }

                if (_continuation != null && _isResolved)
                {
                    ScheduleContinuation(_continuation);
                }
            }
            finally
            {
                base.Unlock();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get
            {
                return _isResolved;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Result
        {
            get
            {
                if (!_isResolved)
                {
                    throw new InvalidOperationException("Cannot retrieve result because operation is still in progress.");
                }

                if (_error != null)
                {
                    throw _error;
                }

                return _result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyDeferred.Result
        {
            get
            {
                return this.Result;
            }
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

        public Exception Error
        {
            get
            {
                if (!_isResolved)
                {
                    throw new InvalidOperationException("Cannot retrieve error because operation is still in progress.");
                }

                return _error;
            }
        }
    }
}
