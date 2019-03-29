using Demos.TodoList.DevOps;
using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;

namespace Demo.HelloWorld
{
    public class StagingEnvironment : GkeEnvironment<TodoListEnvConfig>
    {
        public StagingEnvironment()
            : base(name: "qa", role: "staging", config: new TodoListEnvConfig())
        {
        }
        
        [Include]
        public TodoListDeployment TodoList => new TodoListDeployment(Config); 
    }
}
