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

        public IScheduler Scheduler
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }

}