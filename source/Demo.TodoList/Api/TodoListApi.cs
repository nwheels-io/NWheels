using System;
using System.Net;
using Demo.TodoList.BackEnd;
using Demo.TodoList.Domain;
using NWheels.Composition.Model;
using NWheels.DB.RestApi.Model;
using NWheels.RestApi.Model;

namespace Demo.TodoList.Api
{
    public class TodoListApi : RestApiModel, ITodoListApi
    {
        private readonly TodoListDB _db;
        
        public TodoListApi(TodoListDB db)
        {
            _db = db;
        }
        
        [Include]
        public GraphQLApiRoute<TodoItemEntity> TodoItems => _db.TodoItems.AsGraphQLApiRoute();
    }
}
