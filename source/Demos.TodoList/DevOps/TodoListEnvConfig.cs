using NWheels.DevOps.Model;
using static NWheels.Domain.Model.SemanticContract;
using static NWheels.Domain.Model.ValueContract;

namespace Demos.TodoList.DevOps
{
    public class TodoListEnvConfig
    {
        [Required, Secret, ConnectionString]
        public string DBConnectionString = null;

        [Required, Url]
        public string WebAppUrl = null;
            
        [Required, Url]
        public string RestApiUrl = null;
    }
}