using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;

namespace Demos.TodoList.DevOps
{
    public class StagingEnvironment : GkeEnvironment<TodoListEnvConfig>
    {
        public StagingEnvironment()
            : base(name: "qa", role: "staging", config: new TodoListEnvConfig())
        {
        }
        
        [Include]
        public CommonEnvironment Common => new CommonEnvironment(Config); 
    }
}
