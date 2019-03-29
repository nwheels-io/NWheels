using Demos.TodoList.Api;
using Demos.TodoList.DB;
using NWheels.DevOps.Model;
using NWheels.Composition.Model;

namespace Demos.TodoList.DevOps
{
    public class TodoListService : Microservice<TodoListEnvConfig>
    {
        public TodoListService(TodoListEnvConfig config) : base(
            name: "todo-service", 
            config, 
            new MicroserviceOptions {
                State = StateOption.Stateless,
                Availability = AvailabilityOption.High2Nines  
            })
        {
        }

        [Include]
        public TodoListApi RestApi => new TodoListApi(Database);

        [Include]
        public TodoListDB Database => new TodoListDB();
    }
}
