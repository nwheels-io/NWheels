using System;
using NWheels.Api;
using NWheels.Api.Concurrency;

namespace NWheels.Api.Testing
{
    public class TestFramework : IFramework
    {
        DateTime IFramework.UtcNow
        {
            get
            {
                if (PresetUtcNow.HasValue)
                {
                    return new DateTime(PresetUtcNow.Value.Ticks, DateTimeKind.Utc);
                }
            
                return DateTime.UtcNow;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime? PresetUtcNow { get; set; }

        void IFramework.Go(Action routine)
        {
            throw new NotImplementedException();
        }

        void IFramework.Go<T>(Func<T> routine)
        {
            throw new NotImplementedException();
        }

        IChannel<T> IFramework.MakeChannel<T>()
        {
            throw new NotImplementedException();
        }

        int IFramework.Select(out SelectResult result, params IChannel[] channels)
        {
            throw new NotImplementedException();
        }

        IPromise IFramework.Defer(Action routine)
        {
            throw new NotImplementedException();
        }

        IPromise<T> IFramework.Defer<T>(Func<T> routine)
        {
            throw new NotImplementedException();
        }

        bool IFramework.TrySelect(TimeSpan timeout, Func<ISelectCase, IEndSelect> cases)
        {
            throw new NotImplementedException();
        }
    }
}