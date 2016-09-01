using System;

namespace NWheels.Api.Concurrency
{
    public interface IPromise
    {
        IPromiseBuilder OnTimeout(TimeSpan duration, Action handler);
        IPromiseBuilder OnTimeout(DateTime utc, Action handler);
        IPromiseBuilder<T> OnTimeout<T>(TimeSpan duration, Func<T> handler);
        IPromiseBuilder<T> OnTimeout<T>(DateTime utc, Func<T> handler);
        void ChangeTimeout(Action handler, TimeSpan duration);
        void ChangeTimeout(Action handler, DateTime utc);
        void ExtendTimeout(Action handler, TimeSpan delta);
        void CancelTimeout(Action handler);
        void CancelAllTimeouts();
        void SetDeadline(TimeSpan duration);
        void SetDeadline(DateTime utc);
        void ExtendDeadline(TimeSpan delta);
        void Wait();
        bool Wait(TimeSpan timeout);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromiseBuilder : IPromise
    {
        IPromiseBuilder Then(Action success = null, Action<Exception> failure = null, Action cancel = null, Action finish = null);
        IPromiseBuilder<T> Then<T>(Func<T> success = null, Action<Exception> failure = null, Action cancel = null, Action finish = null);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromiseBuilder<T> : IPromise
    {
        IPromiseBuilder<T2> Then<T2>(Func<T, T2> success = null, Action<Exception> failure = null, Action<T> finish = null);
        IPromise Then(Action<T> success = null, Action<Exception> failure = null, Action<T> finish = null);
    }
}
