using Demos.TodoList.DevOps;
using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;

namespace Demo.HelloWorld
{
    public class ProductionEnvironment : GkeEnvironment<TodoListEnvConfig>
    {
        public ProductionEnvironment()
            : base(name: "prod", role: "prod", config: new TodoListEnvConfig())
        {
        }
        
        [Include]
        public TodoListDeployment TodoList => new TodoListDeployment(Config); 
    }
}
