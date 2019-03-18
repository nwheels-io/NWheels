using GraphQL.Types;
using TodoList.BackendService.Domain;
using TodoList.BackendService.Repositories;

namespace TodoList.BackendService.Schemas
{
    public class TodoListMutation : ObjectGraphType
    {
        public TodoListMutation(ITodoItemRepository repository)
        {
            Field<TodoItemGraph, TodoItem>()
                .Name("create")
                .Argument<NonNullGraphType<TodoItemInputGraph>>("item", "new item data")
                .ResolveAsync(ctx => {
                    var item = ctx.GetArgument<TodoItem>("item");
                    return repository.Create(item.Description, item.Done);
                });
            
            Field<TodoItemGraph, TodoItem>()
                .Name("update")
                .Argument<NonNullGraphType<TodoItemInputGraph>>("item", "item patch data")
                .ResolveAsync(async ctx => {
                    var item = ctx.GetArgument<TodoItem>("item");
                    await repository.Update(item);
                    return new TodoItem { Id = item.Id };
                });

            Field<TodoItemGraph, TodoItem>()
                .Name("delete")
                .Argument<NonNullGraphType<IntGraphType>>("id", "item ID")
                .ResolveAsync(async ctx => {
                    var id = ctx.GetArgument<int>("id");
                    await repository.Delete(new[] { id });
                    return new TodoItem { Id = id };
                });
        }
    }
}
