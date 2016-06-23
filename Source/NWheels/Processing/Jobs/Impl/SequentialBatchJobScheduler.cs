using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NWheels.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Hosting.Core;
using NWheels.Logging;
using NWheels.Processing.Jobs.Core;

namespace NWheels.Processing.Jobs.Impl
{
    public class SequentialBatchJobScheduler : LifecycleEventListenerBase
    {
        private readonly IComponentContext _components;
        private readonly IFramework _framework;
        private readonly IFrameworkApplicationJobConfig _config;
        private readonly IApplicationJobLogger _logger;
        private readonly NodeHost _nodeHost;
        private readonly IEnumerable<AutofacExtensions.ApplicationJobRegistration> _registrations;
        private List<AutofacExtensions.ApplicationJobRegistration> _jobRegistrationSequence;
        private CancellationTokenSource _cancellation;
        private Thread _jobThread;
        private List<IApplicationJob> _jobSequenceToRun;
        private volatile IApplicationJob _currentJob;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SequentialBatchJobScheduler(
            IComponentContext components,
            IFramework framework,
            IFrameworkApplicationJobConfig config, 
            IApplicationJobLogger logger,
            NodeHost nodeHost,
            IEnumerable<AutofacExtensions.ApplicationJobRegistration> registrations)
        {
            _components = components;
            _framework = framework;
            _config = config;
            _logger = logger;
            _nodeHost = nodeHost;
            _registrations = registrations;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost)
        {
            var registrationByJobId = GetRegistrationByJobId(_registrations);
            var jobIdList = GetJobIdList();
            
            _jobRegistrationSequence = GetJobRegistrationSequence(jobIdList, registrationByJobId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Load()
        {
            _jobSequenceToRun = _jobRegistrationSequence
                .Select(r => _components.Resolve(r.JobComponentType))
                .Cast<IApplicationJob>()
                .ToList();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivated()
        {
            _cancellation = new CancellationTokenSource();

            _jobThread = new Thread(() => {
                Thread.Sleep(1000);
                
                _framework.As<ICoreFramework>().RunThreadCode(
                    RunAllJobsInBatch, 
                    () => _logger.ExecutingJobsInBatch()
                );
            });

            _jobThread.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _cancellation.Cancel();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            var stopTimeout = TimeSpan.FromSeconds(30);

            if (_jobThread.Join(stopTimeout))
            {
                _logger.BatchJobSchedulerSuccessfullyStopped();
            }
            else
            {
                var currentJobSnapshot = _currentJob;
                _logger.JobFailedToStopInTimelyFashion(
                    jobId: currentJobSnapshot != null ? currentJobSnapshot.JobId : "???", 
                    timeout: stopTimeout);

                if (_currentJob != null)
                {
                    _jobThread.Abort();
                }
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunAllJobsInBatch()
        {
            _logger.UpAndRunningInBatchJobMode();

            if (_jobSequenceToRun.Count > 0)
            {
                foreach (var job in _jobSequenceToRun)
                {
                    if (_cancellation.IsCancellationRequested)
                    {
                        _logger.JobsBatchCancelled();
                        return;
                    }

                    RunOneJob(job);
                }

                _logger.AllBatchJobsFinishedShuttingDown();
            }
            else
            {
                _logger.NoJobsConfiguredToRun();
            }

            if (!_cancellation.IsCancellationRequested)
            {
                Task.Run(() => _nodeHost.DeactivateAndUnload());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunOneJob(IApplicationJob job)
        {
            using (var activity = _logger.ExecutingJob(job.JobId))
            {
                _currentJob = job;

                try
                {
                    var context = new JobContext(job, _cancellation.Token, _logger);
                    job.Execute(context);
                    _logger.JobSuccessfullyCompleted(job.JobId, ((ActivityLogNode)activity).MillisecondsDuration);
                }
                catch (Exception e)
                {
                    activity.Fail(e);
                    _logger.JobExecutionFailed(job.JobId, ((ActivityLogNode)activity).MillisecondsDuration, exception: e);
                    // swallow and continue to next job
                }
                finally
                {
                    _currentJob = null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string[] GetJobIdList()
        {
            string[] jobIdList;

            if (_config.JobsInBatch != null)
            {
                jobIdList = _config.JobsInBatch.Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                jobIdList = new string[0];
            }

            return jobIdList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Dictionary<string, AutofacExtensions.ApplicationJobRegistration> GetRegistrationByJobId(
            IEnumerable<AutofacExtensions.ApplicationJobRegistration> registrations)
        {
            var registrationByJobId = new Dictionary<string, AutofacExtensions.ApplicationJobRegistration>(StringComparer.OrdinalIgnoreCase);

            foreach (var registration in registrations)
            {
                if (registrationByJobId.ContainsKey(registration.JobId))
                {
                    throw new ConfigurationErrorsException(string.Format(
                        "Duplicate application job id '{0}': originally used by '{1}', then duplicated by '{2}'.",
                        registration.JobId,
                        registrationByJobId[registration.JobId].JobComponentType.FullName,
                        registration.JobComponentType.FullName));
                }

                registrationByJobId.Add(registration.JobId, registration);
            }

            return registrationByJobId;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static List<AutofacExtensions.ApplicationJobRegistration> GetJobRegistrationSequence(
            string[] jobIdList, 
            Dictionary<string, AutofacExtensions.ApplicationJobRegistration> registrationByJobId)
        {
            var jobsToRun = new List<AutofacExtensions.ApplicationJobRegistration>();

            foreach (var jobId in jobIdList)
            {
                var registration = registrationByJobId.GetOrThrow(jobId, "A job is configured to run but it was not registered. Job ID: '{0}'.");

                jobsToRun.Add(registration);
            }

            return jobsToRun;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class JobContext : IApplicationJobContext
        {
            private readonly IApplicationJob _job;
            private readonly IApplicationJobLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public JobContext(IApplicationJob job, CancellationToken cancellation, IApplicationJobLogger logger)
            {
                _job = job;
                _logger = logger;
                
                this.Cancellation = cancellation;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IApplicationJobContext

            public void Report(string statusText, decimal percentCompleted)
            {
                _logger.ReportingJobProgress(_job.JobId, statusText, percentCompleted);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsDryRun
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CancellationToken Cancellation { get; private set; }

            #endregion
        }
    }
}
