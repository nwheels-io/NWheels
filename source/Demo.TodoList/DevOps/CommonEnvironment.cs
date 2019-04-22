using NWheels.Composition.Model;

namespace Demo.TodoList.DevOps
{
    public class CommonEnvironment : PartialModel<TodoListEnvConfig>
    {
        public CommonEnvironment(TodoListEnvConfig config) : base(config)
        {
        }

        [Include]
        public BackEndDeployment BackEnd => new BackEndDeployment(Config); 

        [Include]
        public FrontEndDeployment FrontEnd => new FrontEndDeployment(Config); 
    }
}
