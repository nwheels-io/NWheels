using GraphQL.Types;

namespace TodoList.BackendService.Schemas
{
    public class TodoItemOrderGraph : InputObjectGraphType
    {
        public TodoItemOrderGraph()
        {
            Name = "TodoItemOrder";
            Field<OrderByDirectionType>("id");
            Field<OrderByDirectionType>("description");
            Field<OrderByDirectionType>("done");
        }

        public class Poco
        {
            public OrderByDirection? Id { get; set; }
            public OrderByDirection? Description { get; set; }
            public OrderByDirection? Done { get; set; }
        }
    }
}
