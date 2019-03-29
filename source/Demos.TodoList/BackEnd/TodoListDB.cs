using Demos.TodoList.Domain;
using NWheels.DB.Model;

namespace Demos.TodoList.BackEnd
{
    public class TodoListDB : DatabaseModel
    {
        public DBCollection<TodoItemEntity> TodoItems;
    }
}
