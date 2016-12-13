using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency.Core
{
    public class Deferred<T> : IDeferred<T>, IDeferred
    {
        private bool _isResolved = false;
        private T _result = default(T);
        private Exception _error = null;
        private Action _continuation;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Resolve(T result)
        {
            _isResolved = true;
            _result = result;
            _error = null;

            ScheduleContinuation();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDeferred.Resolve(object result)
        {
            this.Resolve((T)result);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Fail(Exception error)
        {
            _isResolved = true;
            _result = default(T);
            _error = error;

            ScheduleContinuation();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IDeferred<T>.Configure(Action continuation, TimeSpan? timeout, System.Threading.CancellationToken? cancellation)
        {
            _continuation = continuation;
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

        object IDeferred.Result
        {
            get
            {
                return this.Result;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void ScheduleContinuation()
        {
            if (_continuation != null)
            {
                ThreadPool.UnsafeQueueUserWorkItem(
                    state => {
                        _continuation();
                    },
                    null);
            }
        }
    }
}
