using Demos.TodoList.DB;
using Demos.TodoList.Domain;
using NWheels.DB.RestApi.Model;
using NWheels.RestApi.Model;

namespace Demos.TodoList.Api
{
    public class TodoListApi : RestApiModel
    {
        public RestApiRoute<ICrudService<TodoItemEntity>> TodoItem =>
            RestApiRoute.Implementation.GraphQLOverDB<TodoListDB, TodoItemEntity>(
                db => db.TodoItems
            );
    }
}
