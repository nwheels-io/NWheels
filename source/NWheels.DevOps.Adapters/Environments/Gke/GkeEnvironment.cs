using NWheels.DevOps.Model;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public abstract class GkeEnvironment<TConfig> : Environment<TConfig>
    {
        protected GkeEnvironment(string name, string role, TConfig config) 
            : base(name, role, config)
        {
        }
    }
}
