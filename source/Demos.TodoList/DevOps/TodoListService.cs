using NWheels.Deployment.Model;
using NWheels.Package.Model;

namespace Demos.TodoList.DevOps
{
    public class TodoListService : Microservice<TodoListPackage.EnvironmentConfig>
    {
        public TodoListService(TodoListPackage.EnvironmentConfig config) : base(
            name: "todo-service", 
            config, 
            new MicroserviceOptions {
                State = StateOption.Stateless,
                Availability = AvailabilityOption.High2Nines  
            })
        {
        }

        protected override void Contribute(IContributions contributions)
        {
            contributions.IncludePackage(new TodoListPackage(Config));
        }
    }
}
