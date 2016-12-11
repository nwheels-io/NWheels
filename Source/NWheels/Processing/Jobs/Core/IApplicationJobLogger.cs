using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Processing.Jobs.Core
{
    public interface IApplicationJobLogger : IApplicationEventLogger
    {
        [LogInfo]
        void UpAndRunningInBatchJobMode();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void BackgroundJobStarted(string jobId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogThread(ThreadTaskType.ScheduledJob)]
        ILogActivity ExecutingJobsInBatch();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogActivity]
        ILogActivity ExecutingJob(string jobId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void NoJobsConfiguredToRun();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void JobsBatchCancelled();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void ReportingJobProgress(string jobId, string statusText, decimal percentCompleted);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void JobSuccessfullyCompleted(string jobId, long millisecondsDuration);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError]
        void JobExecutionFailed(string jobId, long millisecondsDuration, Exception exception);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void BackgroundJobStopped(string jobId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void AllBatchJobsFinishedShuttingDown();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo]
        void BatchJobSchedulerSuccessfullyStopped();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void JobFailedToStopInTimelyFashion(string jobId, TimeSpan timeout);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void JobExecutionCanceled(string jobId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void JobRunSkippedPreviousRunIsStillInProgress(string jobId);
    }
}
