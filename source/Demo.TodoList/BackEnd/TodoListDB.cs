using Demo.TodoList.Domain;
using NWheels.DB.Model;

namespace Demo.TodoList.BackEnd
{
    public class TodoListDB : DatabaseModel
    {
        public DBCollection<TodoItemEntity> TodoItems;
    }
}
