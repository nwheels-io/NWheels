using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Gke;
using NWheels.DevOps.Model;

namespace Demo.TodoList.DevOps
{
    public class ProductionEnvironment : Environment<TodoListEnvConfig>
    {
        public ProductionEnvironment()
            : base(name: "prod", role: "prod", config: new TodoListEnvConfig())
        {
        }
        
        [Include]
        public CommonEnvironment Common => new CommonEnvironment(Config); 
    }
}
