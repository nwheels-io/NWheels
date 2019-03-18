using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using TodoList.BackendService.Domain;
using TodoList.BackendService.Repositories;

namespace TodoList.BackendService.Schemas
{
    public class TodoListQuery : ObjectGraphType
    {
        public TodoListQuery(ITodoItemRepository repository)
        {
            Field<ListGraphType<TodoItemGraph>>(
                name: "fetch",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id" },
                    new QueryArgument<StringGraphType> { Name = "description" },
                    new QueryArgument<BooleanGraphType> { Name = "done" }
                ),
                resolve: context => {
                    var id = context.GetArgument<int?>("id");
                    var description = context.GetArgument<string>("description");
                    var done = context.GetArgument<bool?>("done");

                    return repository.GetByQuery(id, description, done);
                }
            );
        }
    }
}
