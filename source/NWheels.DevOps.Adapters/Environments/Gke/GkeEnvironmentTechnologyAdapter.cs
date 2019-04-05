using NWheels.Composition.Model.Metadata;
using NWheels.DevOps.Model;
using NWheels.DevOps.Model.Impl.Metadata;

namespace NWheels.DevOps.Adapters.Environments.Gke
{
    public class GkeEnvironmentTechnologyAdapter : ITechnologyAdapter
    {
        public void Execute(ITechnologyAdapterContext context)
        {
            var environment = (EnvironmentMetadata)context.Input;
            
            context.Output.AddSourceFile(
                new[] { "devops" },
                "gke-environment-dummy.txt",
                environment.Dummy
            );
        }
    }
}
