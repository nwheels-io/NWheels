using Demos.TodoList.UI;
using NWheels.Composition.Model;
using NWheels.DB.Adpters.MongoDB;
using NWheels.DevOps.Adapters.Microservices.DotNet;
using NWheels.DevOps.Model;
using NWheels.RestApi.Adapters.AspNetCore;
using NWheels.UI.Adapters.Web.ReactRedux;
using NWheels.UI.Model.Web;

namespace Demos.TodoList.DevOps
{
    public class TodoListDeployment : Deployment<TodoListEnvConfig>
    {
        public TodoListDeployment(TodoListEnvConfig config) 
            : base(config)
        {
        }

        private TodoListService Service => new TodoListService(EnvConfig);

        private TodoListWebApp WebApp => new TodoListWebApp(Service.RestApi);

        [Include]
        public ReactReduxWebApp WebAppComponent => WebApp.AsReactReduxWebApp();

        [Include] 
        public DotNetMicroservice ServiceComponent => Service.AsDotNetMicroservice(
            () => Service.RestApi.AsAspNetCoreServer(),
            () => Service.Database.AsDotNetOdmToMongoDB()
        );

        [Include]
        public MongoDBDatabase DatabaseComponent => Service.Database.AsMongoDBDatabase();
    }
}
