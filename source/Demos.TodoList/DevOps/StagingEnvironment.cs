using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.DevOps.Model;

namespace Demos.TodoList.DevOps
{
    public class StagingEnvironment : Environment<TodoListEnvConfig>
    {
        public StagingEnvironment()
            : base(name: "qa", role: "staging", config: new TodoListEnvConfig())
        {
        }
        
        [Include]
        public CommonEnvironment Common => new CommonEnvironment(Config); 
    }
}
