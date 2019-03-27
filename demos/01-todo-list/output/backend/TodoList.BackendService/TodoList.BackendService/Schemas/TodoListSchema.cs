using GraphQL;
using GraphQL.Types;

namespace TodoList.BackendService.Schemas
{
    public class TodoListSchema: Schema
    {
        public TodoListSchema(IDependencyResolver resolver) 
            : base(resolver)
        {
            Query = resolver.Resolve<TodoListQuery>();
            Mutation = resolver.Resolve<TodoListMutation>();
        }
    }
}
