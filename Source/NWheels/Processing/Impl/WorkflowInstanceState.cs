//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NWheels.Extensions;
//using NWheels.Processing.Core;

//namespace NWheels.Processing.Impl
//{
//    internal enum WorkflowInstanceState
//    {
//        Created = WorkflowState.Created,
//        Initializing,
//        Starting,
//        Running = WorkflowState.Running,
//        Suspending,
//        Suspended = WorkflowState.Suspended,
//        Resuming,
//        Completing,
//        Failing,
//        Finalizing,
//        CompletedTerminated = WorkflowState.Completed,
//        FailedTerminated = WorkflowState.Failed
//    }

//    //---------------------------------------------------------------------------------------------------------------------------------------------------------

//    internal enum WorkflowInstanceTrigger
//    {
//        Run,
//        Resume,
//        Success,
//        Failure,
//        ProcessorCompleted,
//        ProcessorSuspended
//    }

//    //---------------------------------------------------------------------------------------------------------------------------------------------------------

//    internal static class WorkflowInstanceStateExtensions
//    {
//        public static WorkflowState ToWorkflowState(this WorkflowInstanceState instanceState)
//        {
//            if ( instanceState.IsIn(
//                WorkflowInstanceState.Created,
//                WorkflowInstanceState.Running,
//                WorkflowInstanceState.Suspended,
//                WorkflowInstanceState.CompletedTerminated,
//                WorkflowInstanceState.FailedTerminated) )
//            {
//                return (WorkflowState)instanceState;
//            }

//            throw new ArgumentOutOfRangeException("instanceState", "Specified WorkflowInstanceState value has no corresponding WorkflowState value.");
//        }
//    }
//}
