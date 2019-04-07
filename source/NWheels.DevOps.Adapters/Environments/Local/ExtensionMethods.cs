using NWheels.Composition.Model.Impl.Metadata;
using NWheels.DevOps.Model;

namespace NWheels.DevOps.Adapters.Environments.Local
{
    public static class ExtensionMethods
    {
        [TechnologyAdapter(typeof(LocalEnvironmentTechnologyAdapter))]
        public static LocalMachineEnvironment AsLocalMachineEnvironment<TConfig>(
            this Environment<TConfig> environment)
        {
            return new LocalMachineEnvironment();
        }
    }
}