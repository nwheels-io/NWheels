using NWheels.DevOps.Model;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public static class GkeEnvironmentTechnologyAdapter
    {
        public static GkeEnvironment AsGkeEnvironment<TConfig>(this Environment<TConfig> environment)
        {
            return new GkeEnvironment(); 
        }
    }
}
