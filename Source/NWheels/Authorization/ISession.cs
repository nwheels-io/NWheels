using System;
using System.Security.Principal;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization
{
    public interface ISession
    {
        TUser UserAccountAs<TUser>() where TUser : class;
        string Id { get; }
        IPrincipal UserPrincipal { get; }
        IIdentityInfo UserIdentity { get; }
        IEndpoint Endpoint { get; }
        DateTime OpenedAtUtc { get; }
        DateTime? ExpiresAtUtc { get; }
        bool IsGlobalImmutable { get; }
    }
}