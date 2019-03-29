using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;

namespace Demos.TodoList.DevOps
{
    public class ProductionEnvironment : GkeEnvironment<TodoListEnvConfig>
    {
        public ProductionEnvironment()
            : base(name: "prod", role: "prod", config: new TodoListEnvConfig())
        {
        }
        
        [Include]
        public CommonEnvironment Common => new CommonEnvironment(Config); 
    }
}
