using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public struct Promise : IAnyPromise
    {
        private readonly IDeferred _deferred;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise(IDeferred deferred)
        {
            _deferred = deferred;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise Then(Action onSuccess, Action onFailure = null, Action onTimeout = null)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise ConfigureAwait(TimeSpan? timeout = null, CancellationToken? cancellation = null)
        {
            if (_deferred != null)
            {
                _deferred.Configure(timeout: timeout, cancellation: cancellation);
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PromiseAwaiter GetAwaiter()
        {
            return new PromiseAwaiter(_deferred);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyPromise.GetResult()
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyPromise.ResultType
        {
            get
            {
                return null;
            }
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

        public static Promise Resolved()
        {
            return new Promise();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Promise<T> Resolved<T>(T value)
        {
            return new Promise<T>(value);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct Promise<T> : IAnyPromise
    {
        private readonly IDeferred<T> _deferred;
        private readonly T _result;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise(IDeferred<T> deferred)
        {
            _deferred = deferred;
            _result = default(T);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Promise(T result)
        {
            _deferred = null;
            _result = result;
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
            return new PromiseAwaiter<T>(_result, _deferred);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IAnyPromise.GetResult()
        {
            return this.Result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IAnyPromise.ResultType
        {
            get
            {
                return typeof(T);
            }
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

        public T Result
        {
            get
            {
                if (_deferred != null)
                {
                    return _deferred.Result;
                }

                return _result;
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

        public static implicit operator Promise<T>(T result)
        {
            return new Promise<T>(result);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAnyPromise
    {
        object GetResult();
        Type ResultType { get; }
        bool IsResolved { get; }
        Exception Error { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAnyDeferred
    {
        void Configure(Action continuation = null, TimeSpan? timeout = null, CancellationToken? cancellation = null);
        void Resolve(object result);
        void Fail(Exception error);
        Type ResultType { get; }
        object Result { get; }
        bool IsResolved { get; }
        Exception Error { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDeferred
    {
        void Resolve();
        void Fail(Exception error);
        void Configure(Action continuation = null, TimeSpan? timeout = null, CancellationToken? cancellation = null);
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

    public struct PromiseAwaiter : INotifyCompletion
    {
        private readonly IDeferred _deferred;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PromiseAwaiter(IDeferred deferred)
        {
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

        public void GetResult()
        {
            if (_deferred != null)
            {
                if (_deferred.Error != null)
                {
                    throw _deferred.Error;
                }
            }
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
