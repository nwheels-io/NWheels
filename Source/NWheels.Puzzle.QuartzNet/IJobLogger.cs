using System;
using NWheels.Logging;

namespace NWheels.Puzzle.QuartzNet
{
    public interface IJobLogger : IApplicationEventLogger
    {
        [LogVerbose]
        void RegisteringJob(string name);

        [LogThread(ThreadTaskType.ScheduledJob)]
        ILogActivity ExecutingJob(string jobName, string triggerName);
        
        [LogVerbose]
        void JobInvocationDetails(
            string jobInstanceId, 
            [Format("HH:mm:ss"), Detail] DateTimeOffset? triggeredAt,
            [Format("HH:mm:ss"), Detail] DateTimeOffset? previousRunAt,
            [Format("HH:mm:ss"), Detail] DateTimeOffset? nextRunAt);

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