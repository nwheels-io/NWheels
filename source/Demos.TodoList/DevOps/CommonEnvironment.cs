using NWheels.Composition.Model;

namespace Demos.TodoList.DevOps
{
    public class CommonEnvironment : PartialInclude<TodoListEnvConfig>
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
