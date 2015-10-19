using System;

namespace NWheels.Authorization
{
    public interface IRuntimeAccessContext
    {
        ISession Session { get; }
        string UserStory { get; }
        Type ApiContract { get; }
        string ApiOperation { get; }
        Type DomainContext { get; }
    }
}
