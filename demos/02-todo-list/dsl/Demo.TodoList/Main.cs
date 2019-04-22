using Demo.TodoList.DevOps;
using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.DevOps.Adapters.Environments.Local;
using NWheels.DevOps.Model;

namespace Demo.TodoList
{
    public class Main : SystemMain
    {
        private const string GcpZone = "us-central";
        private const string GcpProject = "nwheels-demo";
        
        [Include] 
        GkeEnvironment Production => new ProductionEnvironment().AsGkeEnvironment(GcpZone, GcpProject);

        [Include]
        GkeEnvironment Staging => new StagingEnvironment().AsGkeEnvironment(GcpZone, GcpProject);

        [Include]
        LocalMachineEnvironment Dev => new DevEnvironment().AsLocalMachineEnvironment();
    }
}