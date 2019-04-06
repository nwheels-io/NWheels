using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using GraphQL.Types;
using TodoList.BackendService.Domain;
using TodoList.BackendService.Repositories;

namespace TodoList.BackendService.Schemas
{
    public class TodoListQuery : ObjectGraphType
    {
        private static readonly Func<ResolveFieldContext<object>, QueryFilter<TodoItem>> _whereFilter =
            (context) => (source) => {
                var id = context.GetArgument<int?>("id");
                var description = context.GetArgument<string>("description");
                var done = context.GetArgument<bool?>("done");
                var result = source;

                if (id.HasValue)
                {
                    result = result.Where(x => x.Id == id.Value);
                }
                if (description != null)
                {
                    result = result.Where(x => x.Description.Contains(description));
                }
                if (done.HasValue)
                {
                    result = result.Where(x => x.Done == done.Value);
                }

                return result;
            }; 

        private static readonly Func<ResolveFieldContext<object>, QueryFilter<TodoItem>> _orderFilter =
            (context) => (source) => {
                var orderBy = context.GetArgument<TodoItemOrderGraph.Poco>("orderBy");
                var id = orderBy?.Id;
                var description = orderBy?.Description;
                var done = orderBy?.Done;
                var result = source;

                if (id.HasValue)
                {
                    result = result.OrderBy(x => x.Id, id.Value);
                }

                if (description.HasValue)
                {
                    result = result.OrderBy(x => x.Description, description.Value);
                }

                if (done.HasValue)
                {
                    result = result.OrderBy(x => x.Done, done.Value);
                }

                return result; 
            }; 
            
        public TodoListQuery(ITodoItemRepository repository)
        {
            Field<ListGraphType<TodoItemGraph>>(
                name: "fetch",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "id" },
                    new QueryArgument<StringGraphType> { Name = "description" },
                    new QueryArgument<BooleanGraphType> { Name = "done" },
                    new QueryArgument<TodoItemOrderGraph> { Name = "orderBy" }
                ),
                resolve: context => {

                    return repository.GetByQuery(_whereFilter(context), _orderFilter(context));
                }
            );
            
        }
    }
}
