using System;
using NWheels.Api;

namespace NWheels.Kernel
{
    public class RealFramework : IFramework
    {
        public T NewDomainObject<T>()
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public TRef NewEntityReference<TRef>()
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}