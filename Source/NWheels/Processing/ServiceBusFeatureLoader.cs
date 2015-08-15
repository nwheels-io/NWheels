using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Messages;

namespace NWheels.Processing
{
    public class ServiceBusFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<NWheels.Processing.Messages.Impl.ServiceBus>().SingleInstance();
            builder.NWheelsFeatures().Logging().RegisterLogger<IServiceBusEventLogger>();
        }

        #endregion
    }
}
