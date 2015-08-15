using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Processing
{
    public class WorkflowEngineFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<NWheels.Processing.Workflows.Impl.WorkflowEngine>().SingleInstance();
        }

        #endregion
    }
}
