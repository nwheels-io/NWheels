using Demo.TodoList.Api;
using Demo.TodoList.DevOps;
using NWheels.Composition.Model;
using NWheels.DevOps.Model;

namespace Demo.TodoList.BackEnd
{
    public class TodoListService : Microservice<TodoListEnvConfig>
    {
        public TodoListService(TodoListEnvConfig config) : base(
            "todo-service", 
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
