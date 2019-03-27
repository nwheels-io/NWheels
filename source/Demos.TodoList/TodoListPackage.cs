using Demos.TodoList.Api;
using Demos.TodoList.DB;
using Demos.TodoList.UI;
using NWheels.DB.Model;
using NWheels.DevOps.Model;
using NWheels.Composition.Model;
using NWheels.RestApi.Model;
using NWheels.UI.Model.Web;
using static NWheels.Domain.Model.SemanticContract;
using static NWheels.Domain.Model.ValueContract;

namespace Demos.TodoList
{
    public class TodoListPackage : Package<TodoListPackage.EnvironmentConfig>
    {
        public TodoListPackage(EnvironmentConfig config) 
            : base(config)
        {
        }
            
        protected override void Contribute(IContributions contributions)
        {
            contributions.IncludeDatabase<TodoListDB>(() => new DatabaseConfig { 
                ConnectionString = Config.DBConnectionString
            });
            
            contributions.IncludeApiRoutes<TodoListApi>(config: new ApiRoutesConfig {
                BaseUrl = Config.RestApiUrl
            });

            contributions.IncludeWebApp<TodoListWebApp>(config: new WebAppConfig {
                BaseUrl = Config.WebAppUrl
            });
        }
        
        public class EnvironmentConfig
        {
            [Required, Secret, ConnectionString]
            public string DBConnectionString = null;

            [Required, Url]
            public string WebAppUrl = null;
            
            [Required, Url]
            public string RestApiUrl = null;
        }
    }
}
