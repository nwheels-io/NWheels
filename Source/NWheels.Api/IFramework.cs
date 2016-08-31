using System;
using NWheels.Api.Concurrency;

namespace NWheels.Api
{
    public interface IFramework
    {
        void Go(Action routine);
        void Go<T>(Func<T> routine);
        IChannel<T> MakeChannel<T>();
        int Select(out SelectResult result, params IChannel[] channels);
        IPromise Defer(Action routine);
        IPromise<T> Defer<T>(Func<T> routine);
        bool TrySelect(TimeSpan timeout, Func<ISelectCase, IEndSelect> cases);
        DateTime UtcNow { get;}
    }
}
