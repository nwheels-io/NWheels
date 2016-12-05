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
using NWheels.Logging;
using NWheels.Logging.Core;
using NWheels.Samples.SimpleChat.Contracts;
using NWheels.Stacks.Nlog;

namespace NWheels.Samples.SimpleChat.Server
{
    public class ModuleLoader : Autofac.Module
    {
        #region Overrides of Module

        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Hosting().RegisterLifecycleComponent<ServerLifecycle>();

            // we have no DB
            builder.RegisterType<ClientSideFramework.VoidStorageInitializer>().As<IStorageInitializer>();
            
            builder.RegisterType<ChatService>().As<IChatServiceApi>().InstancePerDependency();

            builder.NWheelsFeatures().Configuration().ProgrammaticSection<IFrameworkLoggingConfiguration>(config => {
                config.Level = LogLevel.Debug;
                NLogBasedPlainLog.Instance.ConsoleLogLevel = NLog.LogLevel.Debug;
            });
        }

        #endregion
    }
}
