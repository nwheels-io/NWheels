using System;

namespace NWheels.Api.Concurrency
{
    public interface IScheduler
    {
        void Go(Action routine);
        IChannel<T> NewChannel<T>(string name);
        int Select(out object value, params IChannel[] channels);
        int Select<T>(out T value, params IConsumer<T>[] channels);
        int TrySelect(TimeSpan timeout, out object value, params IChannel[] channels);
        int TrySelect<T>(TimeSpan timeout, out T value, params IConsumer<T>[] channels);
        IPromise Defer(Action routine);
        IPromise<T> Defer<T>(Func<T> routine);
    }
}
