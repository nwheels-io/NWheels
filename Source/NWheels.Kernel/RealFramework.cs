using System;
using NWheels.Api;
using NWheels.Api.Concurrency;

namespace NWheels.Kernel
{
    public class RealFramework : IFramework
    {
        public IScheduler Scheduler
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DateTime UtcNow
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}