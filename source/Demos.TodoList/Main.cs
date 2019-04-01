using Demos.TodoList.DevOps;
using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.DevOps.Adapters.Environments.Local;
using NWheels.DevOps.Model;

namespace Demos.TodoList
{
    public class Main : SystemMain
    {
        [Include] 
        GkeEnvironment Production => new ProductionEnvironment().AsGkeEnvironment();

        [Include]
        GkeEnvironment Staging => new StagingEnvironment().AsGkeEnvironment();

        [Include]
        LocalMachineEnvironment Dev => new DevEnvironment().AsLocalMachineEnvironment();
    }
}