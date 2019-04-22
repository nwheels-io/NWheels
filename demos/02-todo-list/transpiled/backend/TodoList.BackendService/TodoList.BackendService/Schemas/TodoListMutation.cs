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
                .Argument<NonNullGraphType<TodoItemInputGraph>>("data", "new item data")
                .ResolveAsync(ctx => {
                    var item = ctx.GetArgument<TodoItem>("data");
                    return repository.Create(item.Description, item.Done);
                });
            
            Field<TodoItemGraph, TodoItem>()
                .Name("update")
                .Argument<NonNullGraphType<TodoItemInputGraph>>("data", "item patch data")
                .ResolveAsync(async ctx => {
                    var patch = ctx.GetArgument<TodoItemPatch>("data");
                    await repository.Update(patch);
                    return new TodoItem { Id = patch.Id };
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
