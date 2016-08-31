using System;
using NWheels.Api;
using NWheels.Api.Concurrency;

namespace NWheels.Kernel
{
    public class RealFramework : IFramework
    {
        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public IPromise Defer(Action routine)
        {
            throw new NotImplementedException();
        }

        public IPromise<T> Defer<T>(Func<T> routine)
        {
            throw new NotImplementedException();
        }

        public void Go(Action routine)
        {
            throw new NotImplementedException();
        }

        public void Go<T>(Func<T> routine)
        {
            throw new NotImplementedException();
        }

        public IChannel<T> MakeChannel<T>()
        {
            throw new NotImplementedException();
        }

        public int Select(out SelectResult result, params IChannel[] channels)
        {
            throw new NotImplementedException();
        }

        public bool TrySelect(TimeSpan timeout, Func<ISelectCase, IEndSelect> cases)
        {
            throw new NotImplementedException();
        }
    }
}