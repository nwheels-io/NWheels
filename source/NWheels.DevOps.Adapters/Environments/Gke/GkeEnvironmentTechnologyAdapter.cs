using NWheels.DevOps.Model;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public static class GkeEnvironmentTechnologyAdapter
    {
        public static GkeEnvironment AsGkeEnvironment<TConfig>(
            this Environment<TConfig> environment,
            string zone,
            string project)
        {
            return new GkeEnvironment(); 
        }
    }
}
