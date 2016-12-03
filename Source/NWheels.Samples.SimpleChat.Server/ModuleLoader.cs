using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Client;
using NWheels.Endpoints;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.Server
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DuplexTcpServerFactory>().SingleInstance();
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<ServerLifecycle>();

            // we have no DB
            builder.RegisterType<ClientSideFramework.VoidStorageInitializer>().As<IStorageInitializer>();
            
            builder.RegisterType<ChatService>().As<IChatServiceApi>().InstancePerDependency();
        }

        #endregion
    }
}
