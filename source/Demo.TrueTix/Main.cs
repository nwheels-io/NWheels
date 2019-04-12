using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.DevOps.Model;
using Demo.TrueTix.DevOps;

namespace Demo.TrueTix
{
    public class Main : SystemMain
    {
        [Include]
        public GkeEnvironment Production => new ProductionEnvironment().AsGkeEnvironment(
            zone: "us-central1-a",
            project: "galvanic-wall-235207"
        );
    }
}
