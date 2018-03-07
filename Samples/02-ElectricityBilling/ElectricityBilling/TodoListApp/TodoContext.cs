using System;
using System.Collections.Generic;
using System.Text;
using NWheels.DB;
using NWheels.Ddd;

namespace TodoListApp
{
    [NWheels.Ddd.TypeContract.BoundedContext]
    public class TodoContext
    {
        private readonly IRepository<TodoItem> _items;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TodoContext(IRepository<TodoItem> items)
        {
            _items = items;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [NWheels.Ddd.TypeContract.Aggregate]
    public class TodoItem
    {
        public int Id { get; }

        [NWheels.MemberContract.Required]
        public string Description { get; }

        [NWheels.MemberContract.Semantics.OrderBy]
        public int Order { get; set; }

        public bool Done { get; set; }
    }
}
