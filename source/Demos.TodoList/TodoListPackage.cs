using Demos.TodoList.Api;
using Demos.TodoList.DB;
using Demos.TodoList.UI;
using NWheels.DB.Model;
using NWheels.Deployment.Model;
using NWheels.Domain.Model;
using NWheels.Package.Model;
using NWheels.RestApi.Model;
using NWheels.UI.Model.Web;

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
            [Secret, Required]
            public string DBConnectionString = null;
            [Required]
            public string WebAppUrl = null;
            [Required]
            public string RestApiUrl = null;
        }
    }
}