using Demos.TodoList.UI;
using NWheels.Composition.Model;
using NWheels.DB.Adpters.MongoDB;
using NWheels.DevOps.Adapters.Microservices.DotNet;
using NWheels.DevOps.Model;
using NWheels.RestApi.Adapters.AspNetCore;
using NWheels.UI.Adapters.Web.ReactRedux;

namespace Demos.TodoList.DevOps
{
    public class TodoListDeployment : Deployment<TodoListEnvConfig>
    {
        public TodoListDeployment(TodoListEnvConfig config) 
            : base(config)
        {
        }

        private TodoListService Service => new TodoListService(EnvConfig);

        [Include]
        public ReactReduxWebApp WebAppComponent => new TodoListWebApp().AsReactReduxWebApp();

        [Include] 
        public DotNetMicroservice MicroserviceComponent => Service.AsDotNetMicroservice(
            () => Service.RestApi.AsAspNetCoreServer(),
            () => Service.Database.AsDotNetOdmToMongoDB()
        );

        [Include]
        public MongoDBDatabase DatabaseComponent => Service.Database.AsMongoDBDatabase();
    }
}
