using GraphQL.Types;
using TodoList.BackendService.Domain;

namespace TodoList.BackendService.Schemas
{
    public class TodoItemInputGraph : InputObjectGraphType
    {
        public TodoItemInputGraph()
        {
            Name = "TodoItemInput";
            Field<IntGraphType>("id");
            Field<StringGraphType>("description");
            Field<BooleanGraphType>("done");
        }
    }
}
