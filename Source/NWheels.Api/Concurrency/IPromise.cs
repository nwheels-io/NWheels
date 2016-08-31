using System;

namespace NWheels.Api.Concurrency
{
    public interface IEndPromise
    {
        void Wait();
        bool Wait(TimeSpan timeout);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromise : IEndPromise
    {
        IPromise Then(Action success = null, Action<Exception> failure = null, Action finish = null);
        IPromise<T> Then<T>(Func<T> success = null, Func<Exception, T> failure = null, Action finish = null);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPromise<T> : IEndPromise
    {
        IPromise<T2> Then<T2>(Func<T, T2> success = null, Action<T, T2> failure = null, Action<T> finish = null);
        IEndPromise Then(Action<T> success = null, Action<T> failure = null, Action<T> finish = null);
    }
}
