using System;

namespace NWheels.Api.Concurrency
{
    public interface IEndPromise
    {
        IPromise OnTimeout(TimeSpan duration, Action handler);
        IPromise OnTimeout(DateTime utc, Action handler);
        IPromise<T> OnTimeout<T>(TimeSpan duration, Func<T> handler);
        IPromise<T> OnTimeout<T>(DateTime utc, Func<T> handler);
        void SetTimeout(TimeSpan duration);
        void SetTimeout(DateTime utc);
        void ExtendTimeoutBy(TimeSpan delta);
        void CancelTimeout();
        void Wait();
        bool Wait(TimeSpan timeout);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromise : IEndPromise
    {
        IPromise Then(Action success = null, Action<Exception> failure = null, Action finish = null);
        IPromise<T> Then<T>(Func<T> success = null, Action<Exception> failure = null, Action finish = null);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromise<T> : IEndPromise
    {
        IPromise<T2> Then<T2>(Func<T, T2> success = null, Action<Exception> failure = null, Action<T> finish = null);
        IEndPromise Then(Action<T> success = null, Action<Exception> failure = null, Action<T> finish = null);
    }
}
