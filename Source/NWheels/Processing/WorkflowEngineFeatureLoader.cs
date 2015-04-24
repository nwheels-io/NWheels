using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Processing.Core;
using NWheels.Processing.Impl;

namespace NWheels.Processing
{
    public class WorkflowEngineFeatureLoader : Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Entities().RegisterDataRepository<IWorkflowDataRepository>();
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<WorkflowEngine>().As<IWorkflowEngine>();
        }

        #endregion
    }
}
