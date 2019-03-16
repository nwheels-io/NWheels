using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using TodoList.BackendService.Domain;
using TodoList.BackendService.Graphs;

namespace TodoList.BackendService.Queries
{
    public class TodoItemQuery : ObjectGraphType
    {
        public TodoItemQuery()
        {
            Field<ListGraphType<TodoItemGraph>>(
                name: "todoItem",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id" },
                    new QueryArgument<StringGraphType> { Name = "description" },
                    new QueryArgument<BooleanGraphType> { Name = "done" }
                ),
                resolve: context => {
                    var id = context.GetArgument<int?>("id");
                    var description = context.GetArgument<string>("description");
                    var done = context.GetArgument<bool?>("done");

                    return FindTodoItems(id, description, done);
                }
            );
        }

        private static IEnumerable<TodoItem> FindTodoItems(int? id, string description, bool? done)
        {
            return _exampleData.Where(item =>    
                (!id.HasValue || id.Value == item.Id) &&
                (description == null || item.Description.Contains(description)) &&
                (!done.HasValue || done.Value == item.Done)
            );
        }

        private static readonly TodoItem[] _exampleData = new[] {
            new TodoItem(111) {Description = "First", Done = false},
            new TodoItem(222) {Description = "Second", Done = true},
            new TodoItem(333) {Description = "Third", Done = false},
        };
    }
}
