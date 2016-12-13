using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public struct Promise<T> : IPromise
    {
        private readonly IDeferred<T> _deferred;
        private readonly T _value;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise(IDeferred<T> deferred)
        {
            _deferred = deferred;
            _value = default(T);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise(T value)
        {
            _deferred = null;
            _value = value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise<T> Then(Action onSuccess, Action onFailure = null, Action onTimeout = null)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise<T> ConfigureAwait(TimeSpan? timeout = null, CancellationToken? cancellation = null)
        {
            if (_deferred != null)
            {
                _deferred.Configure(timeout: timeout, cancellation: cancellation);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PromiseAwaiter<T> GetAwaiter()
        {
            return new PromiseAwaiter<T>(_value, _deferred);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IPromise.GetValue()
        {
            return this.Value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsResolved
        {
            get
            {
                if (_deferred != null)
                {
                    return _deferred.IsResolved;
                }

                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Value
        {
            get
            {
                if (_deferred != null)
                {
                    return _deferred.Result;
                }

                return _value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Exception Error
        {
            get
            {
                if (_deferred != null)
                {
                    return _deferred.Error;
                }

                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public T Value
        //{
        //    get
        //    {
        //        if (!_resolved)
        //        {
        //            throw new InvalidOperationException("Cannot retrieve Value because current Promise is not yet resolved.");
        //        }

        //        if (_error != null)
        //        {
        //            throw _error;
        //        }

        //        return _value;
        //    }
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public Exception Error
        //{
        //    get
        //    {
        //        if (!_resolved)
        //        {
        //            throw new InvalidOperationException("Cannot retrieve Error because current Promise is not yet resolved.");
        //        }

        //        return _error;
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator Promise<T>(T value)
        {
            return new Promise<T>();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromise
    {
        object GetValue();
        bool IsResolved { get; }
        Exception Error { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDeferred<T>
    {
        void Resolve(T result);
        void Fail(Exception error);
        void Configure(Action continuation = null, TimeSpan? timeout = null, CancellationToken? cancellation = null);
        bool IsResolved { get; }
        T Result { get; }
        Exception Error { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDeferred
    {
        void Resolve(object result);
        void Fail(Exception error);
        object Result { get; }
        bool IsResolved { get; }
        Exception Error { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct PromiseAwaiter<T> : INotifyCompletion
    {
        private readonly T _value;
        private readonly IDeferred<T> _deferred;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PromiseAwaiter(T value, IDeferred<T> deferred)
        {
            _value = value;
            _deferred = deferred;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of INotifyCompletion

        public void OnCompleted(Action continuation)
        {
            if (_deferred != null)
            {
                _deferred.Configure(continuation: continuation);
            }
            else
            {
                throw new InvalidOperationException("Current awaiter cannot be assigned continuation because it completed synchronously.");
            }
        }
    

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetResult()
        {
            if (_deferred != null)
            {
                return _deferred.Result;
            }
            
            return _value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsCompleted
        {
            get
            {
                if (_deferred != null)
                {
                    return _deferred.IsResolved;
                }

                return true;
            }
        }
    }
}
