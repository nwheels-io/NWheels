using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing;
using Quartz;

namespace NWheels.Puzzle.QuartzNet
{
    [DisallowConcurrentExecution]
    internal class QuartzJobAdapter : IInterruptableJob
    {
        private readonly IApplicationJob _jobComponent;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public QuartzJobAdapter(IApplicationJob jobComponent)
        {
            _jobComponent = jobComponent;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute(IJobExecutionContext context)
        {

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Interrupt()
        {
            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationJob JobComponent
        {
            get { return _jobComponent; }
        }
    }
}
