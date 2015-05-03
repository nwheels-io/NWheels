#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Processing;
using NWheels.Processing.Impl;

namespace NWheels.UnitTests.Processing.Impl
{
    [TestFixture]
    public class WorkflowProcessorTests
    {
        [Test]
        public void CanExecuteSingleActorWorkflow()
        {
            //-- Arrange

            var context = new WorkflowTestHelpers.ProcessorContext();
            var processor = new WorkflowProcessor(context);
            var builder = (IWorkflowBuilder)processor;

            builder.AddActor();

            //-- 


        }
    }
}

#endif