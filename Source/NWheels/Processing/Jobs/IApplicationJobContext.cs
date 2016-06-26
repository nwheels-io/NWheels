using System.Threading;

namespace NWheels.Processing.Jobs
{
    public interface IApplicationJobContext
    {
        void Report(string statusText, decimal? percentCompleted = null);
        bool IsDryRun { get; }
        CancellationToken Cancellation { get; }
    }
}
