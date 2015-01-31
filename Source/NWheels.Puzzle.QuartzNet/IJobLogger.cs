using System;
using NWheels.Logging;

namespace NWheels.Puzzle.QuartzNet
{
    internal interface IJobLogger : IApplicationEventLogger
    {
        [LogVerbose]
        void RegisteringJob(string name);

        [LogThread(ThreadTaskType.ScheduledJob)]
        ILogActivity ExecutingJob(string jobName, string triggerName);
        
        [LogVerbose]
        void JobInvocationDetails(string jobInstanceId, DateTimeOffset? triggeredAt, DateTimeOffset? previousRunAt, DateTimeOffset? nextRunAt);

        [LogVerbose]
        void JobCompleted(string jobName);
 
        [LogError]
        void JobFailed(string jobName, Exception e);

        [LogVerbose]
        void WaitingForJobsToFinish(string jobNames);

        [LogInfo]
        void AllJobsFinished();

        [LogWarning]
        void SomeJobsDidNotFinish(string jobNames);
    }
}