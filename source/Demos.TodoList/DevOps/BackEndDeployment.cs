using Demos.TodoList.BackEnd;
using NWheels.Composition.Model;
using NWheels.DB.Adpters.MongoDB;
using NWheels.DevOps.Adapters.Microservices.DotNet;
using NWheels.DevOps.Model;
using NWheels.RestApi.Adapters.AspNetCore;
using NWheels.UI.Adapters.Web.ReactRedux;
using NWheels.UI.Model.Web;

namespace Demos.TodoList.DevOps
{
    public class BackEndDeployment : Deployment<TodoListEnvConfig>
    {
        public BackEndDeployment(TodoListEnvConfig config) 
            : base(config)
        {
        }

        private TodoListService TodoService => new TodoListService(EnvConfig);

        [Include] 
        public DotNetMicroservice ServiceComponent => TodoService.AsDotNetMicroservice(
            () => TodoService.RestApi.AsAspNetCoreServer(),
            () => TodoService.Database.AsDotNetOdmToMongoDB()
        );

        [Include]
        public MongoDBDatabase DatabaseComponent => TodoService.Database.AsMongoDBDatabase();
    }
}
