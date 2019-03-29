using Demos.TodoList.FrontEnd;
using NWheels.Composition.Model;
using NWheels.DB.Adpters.MongoDB;
using NWheels.DevOps.Adapters.Microservices.DotNet;
using NWheels.DevOps.Model;
using NWheels.RestApi.Adapters.AspNetCore;
using NWheels.UI.Adapters.Web.ReactRedux;
using NWheels.UI.Model.Web;

namespace Demos.TodoList.DevOps
{
    public class FrontEndDeployment : Deployment<TodoListEnvConfig>
    {
        public FrontEndDeployment(TodoListEnvConfig config) 
            : base(config)
        {
        }

        [Include]
        public ReactReduxWebApp WebAppComponent => new TodoListWebApp(EnvConfig.Urls).AsReactReduxWebApp();
    }
}
