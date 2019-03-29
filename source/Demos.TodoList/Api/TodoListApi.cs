using System;
using System.Net;
using Demos.TodoList.DB;
using Demos.TodoList.Domain;
using NWheels.Composition.Model;
using NWheels.DB.RestApi.Model;
using NWheels.RestApi.Model;

namespace Demos.TodoList.Api
{
    public class TodoListApi : RestApiModel
    {
        private readonly TodoListDB _db;
        
        public TodoListApi(TodoListDB db)
        {
            _db = db;
        }
        
        [Include]
        public GraphQLApiRoute<TodoItemEntity> TodoItem => _db.TodoItems.AsGraphQLApiRoute();
    }
}
