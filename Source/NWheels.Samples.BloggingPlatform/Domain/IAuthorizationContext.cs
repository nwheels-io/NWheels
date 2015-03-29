using System.Collections.Generic;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public interface IAuthorizationContext<TRole>
    {
        bool Authorize(IEnumerable<TRole> allowedRoles);
    }
}