using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;
using NWheels.Processing.Core;
using NWheels.Processing.Impl;

namespace NWheels.UnitTests.Processing.Impl
{
    internal static class WorkflowTestHelpers
    {
        public class ProcessorContext : IWorkflowProcessorContext
        {
            private readonly ILogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessorContext(ILogger logger)
            {
                _logger = logger;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IWorkflowProcessorContext.AwaitEvent(Type eventType, object eventKey)
            {
                _logger.ProcessorCalledAwaitEvent(eventType, eventKey);
            }


            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IWorkflowInstance IWorkflowProcessorContext.WorkflowInstance
            {
                get { throw new NotImplementedException(); }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            void ProcessorCalledAwaitEvent(Type eventType, object eventKey);
        }
    }
}
