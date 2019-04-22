using Demo.TodoList.Domain;
using NWheels.RestApi.Model;

namespace Demo.TodoList.Api
{
    public interface ITodoListApi
    {
        GraphQLApiRoute<TodoItemEntity> TodoItems { get; }
    }
}