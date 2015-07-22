using Autofac;
using NWheels.Extensions;

namespace NWheels.Samples.SimpleChatApp
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(Autofac.ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Api().RegisterContract<IChatService>().WithNetworkEndpoint();
            builder.RegisterType<ChatService>().As<IChatService>().SingleInstance();
        }
    }
}
