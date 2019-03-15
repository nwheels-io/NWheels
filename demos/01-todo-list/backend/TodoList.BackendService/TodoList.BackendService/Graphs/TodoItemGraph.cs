using GraphQL.Types;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Graphs
{
    public class TodoItemGraph : ObjectGraphType<TodoItem>
    {
        public TodoItemGraph()
        {
            Field(x => x.Id);
            Field(x => x.Description);
            Field(x => x.Done);
        }
    }
}