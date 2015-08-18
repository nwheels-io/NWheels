using NWheels.Logging;

namespace NWheels.Domains.Security.Core
{
    public interface ISecurityDomainLogger : IApplicationEventLogger
    {
        [LogInfo]
        void UserAuthenticated(string loginName);

        [LogWarning]
        void FailedLoginAttempt(LoginFault fault, string loginName);

        [LogWarning]
        void UserAccountLockedOut(string loginName);

        [LogWarning]
        void UserNotFound(string loginName);
    }
}
