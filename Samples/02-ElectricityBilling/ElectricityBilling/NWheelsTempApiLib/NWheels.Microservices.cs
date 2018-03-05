using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    namespace Microservices
    {
        public interface IComponentContainer
        {
        }

        public interface IComponentContainerBuilder
        {
            IComponentContainerBuilder RegisterComponentType<T>();
            IComponentContainerBuilder InstancePerDependency();
            IComponentContainerBuilder ForService<T>();
        }

        public abstract class LifecycleComponentBase
        {
            public abstract void Activate();
        }

        public static class Microservice
        {
            public static int RunDaemonCli(string name, string[] arguments, Action<MicroserviceHostBuilder> builder)
            {
                var host = BuildHost(name, builder);
                return host.RunCli(arguments);
            }

            public static int RunBatchJobCli<TJob, TArguments>(
                string name,
                string[] arguments,
                Action<MicroserviceHostBuilder> builder)
            {
                var host = BuildHost(name, builder);
                return host.RunCli(arguments);
            }

            public static MicroserviceHost BuildHost(string name, Action<MicroserviceHostBuilder> builder)
            {
                return new MicroserviceHost();
            }
        }

        public class MicroserviceHostBuilder
        {
            public MicroserviceHostBuilder UseLogging<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseDB<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseDdd()
            {
                return this;
            }

            public MicroserviceHostBuilder UseApplicationFeature<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseMicroserviceXml(string path)
            {
                return this;
            }

            public MicroserviceHostBuilder UseLifecycleComponent<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseRestApi<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseUidl<T>()
            {
                return this;
            }

            public MicroserviceHostBuilder UseComponents(Action<IComponentContainer, IComponentContainerBuilder> action)
            {
                throw new NotImplementedException();
            }
        }

        public class DefaultFeatureLoaderAttribute : Attribute
        {
        }

        public abstract class FeatureLoaderBase
        {
            public abstract void ContributeComponents(
                IComponentContainer existingComponents,
                IComponentContainerBuilder newComponents);
        }

        public class AutoDiscoverAssemblyOf<T> : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                throw new NotImplementedException();
            }
        }

        public class MicroserviceHost
        {
            public int RunCli(string[] arguments)
            {
                return 0;
            }

            public Task<int> RunCliAsync(string[] arguments)
            {
                return Task.FromResult(0);
            }
        }
    }

}
