using NWheels.Logging;

namespace NWheels.Stacks.EntityFramework
{
    public interface IDbCommandLogger : IApplicationEventLogger
    {
        [LogDebug]
        void ExecutingSql([Detail(MaxStringLength = 1024, IncludeInSingleLineText = true)] string statement);
    }
}
