using NWheels.Composition.Model;
using NWheels.DevOps.Adapters.Environments.Local;

namespace Demos.TodoList.DevOps
{
    public class DevEnvironment : LocalMachineEnvironment<TodoListEnvConfig>
    {
        public DevEnvironment()
            : base(name: "local", role: "dev", config: DefaultLocalConfig)
        {
        }
        
        [Include]
        public CommonEnvironment Common => new CommonEnvironment(Config);

        private static TodoListEnvConfig DefaultLocalConfig => new TodoListEnvConfig {
            Urls = new TodoListUrlsConfig {
                WebAppUrl = "http://localhost:3000",
                BackendApiUrl = "http://localhost:3001"
            },
            DBConnectionString = "mongodb://localhost" 
        };
    }
}
