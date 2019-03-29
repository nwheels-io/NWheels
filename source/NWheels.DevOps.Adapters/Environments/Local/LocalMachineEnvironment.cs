using NWheels.DevOps.Model;

namespace NWheels.DevOps.Adapters.Environments.Local
{
    public abstract class LocalMachineEnvironment<TConfig> : Environment<TConfig>
    {
        protected LocalMachineEnvironment(string name, string role, TConfig config) 
            : base(name, role, config)
        {
        }
    }
}
