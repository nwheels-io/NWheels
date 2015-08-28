using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Extensions;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;

namespace NWheels.Processing
{
    public class ServiceBusFeatureLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<ServiceBus>().As<ServiceBus, IServiceBus>().SingleInstance();
            builder.NWheelsFeatures().Logging().RegisterLogger<IServiceBusEventLogger>();

            builder.NWheelsFeatures().Processing().RegisterActor<CommandActor>().SingleInstance();
            builder.NWheelsFeatures().Processing().RegisterMessage<CommandResultMessage>();
            builder.NWheelsFeatures().Logging().RegisterLogger<ICommandActorLogger>();

            builder.NWheelsFeatures().Processing().RegisterActor<SessionPushActor>().SingleInstance();
        }

        #endregion
    }
}
