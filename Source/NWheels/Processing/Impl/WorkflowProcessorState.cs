using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Processing.Core;

namespace NWheels.Processing.Impl
{
    internal enum WorkflowProcessorState
    {
        Created = WorkflowState.Created,
        Initializing,
        Starting,
        Running = WorkflowState.Running,
        Suspending,
        Suspended = WorkflowState.Suspended,
        Resuming,
        Completing,
        Failing,
        Finalizing,
        CompletedTerminated = WorkflowState.Completed,
        FailedTerminated = WorkflowState.Failed
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal enum WorkflowProcessorTrigger
    {
        Run,
        Resume,
        Success,
        Failure,
        Completed,
        Suspended
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal static class WorkflowInstanceStateExtensions
    {
        public static WorkflowState ToWorkflowState(this WorkflowProcessorState instanceState)
        {
            if ( instanceState.IsIn(
                WorkflowProcessorState.Created,
                WorkflowProcessorState.Running,
                WorkflowProcessorState.Suspended,
                WorkflowProcessorState.CompletedTerminated,
                WorkflowProcessorState.FailedTerminated) )
            {
                return (WorkflowState)instanceState;
            }

            throw new ArgumentOutOfRangeException("instanceState", "Specified WorkflowInstanceState value has no corresponding WorkflowState value.");
        }
    }
}
