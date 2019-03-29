using Demos.TodoList.Domain;
using NWheels.RestApi.Model;

namespace Demos.TodoList.Api
{
    public interface ITodoListApi
    {
        GraphQLApiRoute<TodoItemEntity> TodoItems { get; }
    }
}