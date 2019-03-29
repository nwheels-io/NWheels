using NWheels.DevOps.Model;
using static NWheels.Domain.Model.SemanticContract;
using static NWheels.Domain.Model.ValueContract;

namespace Demos.TodoList.DevOps
{
    public class TodoListUrlsConfig
    {
        [Required, Url, FromEnvVar, FromCliArg]
        public string WebAppUrl = null;
            
        [Required, Url, FromEnvVar, FromCliArg]
        public string BackendApiUrl = null;
    }

    public class TodoListEnvConfig
    {
        [Required] 
        public TodoListUrlsConfig Urls = null; 
        
        [Required, Secret, ConnectionString, FromEnvVar, FromCliArg]
        public string DBConnectionString = null;
    }
}
