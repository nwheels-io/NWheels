using Demos.TodoList.Domain;
using NWheels.DB.Model;

namespace Demos.TodoList.DB
{
    public class TodoListDB : DatabaseModel
    {
        public DBCollection<TodoItemEntity> TodoItems;
    }
}
