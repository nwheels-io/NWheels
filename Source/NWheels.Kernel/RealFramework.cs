using System;
using NWheels.Api;

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
    }
}