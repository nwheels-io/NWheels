using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing;
using System.Threading;
using NWheels.Processing.Jobs;
using Quartz;

namespace NWheels.Stacks.QuartzNet
{
    internal class QuartzJobContextAdapter : IApplicationJobContext, IDisposable
    {
        private readonly IJobExecutionContext _quartzContext;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly CancellationToken _cancellationToken;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public QuartzJobContextAdapter(IJobExecutionContext quartzContext)
        {
            _quartzContext = quartzContext;
            _cancellationSource = new CancellationTokenSource();
            _cancellationToken = _cancellationSource.Token;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _cancellationSource.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Report(string statusText, decimal? percentCompleted = null)
        {
            //
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsDryRun
        {
            get
            {
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CancellationToken Cancellation
        {
            get
            {
                return _cancellationToken;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Interrupt()
        {
            _cancellationSource.Cancel(throwOnFirstException: false);
        }
    }
}
