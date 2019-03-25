using GraphQL.Types;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Schemas
{
    public class TodoItemGraph : ObjectGraphType<TodoItem>
    {
        public TodoItemGraph()
        {
            Name = "TodoItem";
            
            Field(x => x.Id);
            Field(x => x.Description);
            Field(x => x.Done);
        }
    }
}