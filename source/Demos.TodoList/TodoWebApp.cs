//using NWheels.DB.Model;
//using NWheels.DB.RestApi.Model;
//using NWheels.Deployment.Model;
//using NWheels.Domain.Model;
//using NWheels.Package.Model;
//using NWheels.RestApi.Model;
//using NWheels.UI.Model;
//using NWheels.UI.Model.Web;
//using NWheels.UI.Model.Web.Templates;
//using NWheels.UI.RestApi.Model;
//
//namespace Demos.TodoList
//{
//    public class TodoItem
//    {
//        public readonly int Id;
//        
//        [MaxLength(100)]
//        public string Title;
//
//        public bool Done;
//    }
//
//    public class TodoWebApp : WebApp<Empty.Props, Empty.State>
//    {
//        [IndexPage]
//        public TodoPage Todos => new TodoPage();
//    }
//
//    public class TodoPage : SoloComponentPage<Empty.Props, Empty.State>
//    {
//        public override UIComponent SoloComponent => new GridLayout(props => props
//            .Rows(2).Cols(1)
//            .Cell(0, 0, NewTodoForm)
//            .Cell(1, 0, TodoGrid)
//        );
//        
//        Form<TodoItem> NewTodoForm => new Form<TodoItem>(props => props
//            .WithFields(t => t.Title)
//        );
//        
//        DataGrid<TodoItem> TodoGrid => new DataGrid<TodoItem>(props => props
//            .WithAutoColumns()
//            .WithInlineEditor()
//            .WithAppenderForm(NewTodoForm)
//            .WithDataSource(factory => factory.BackendRestApiCrud())
//        );
//    }
//
//    public class TodoRestApi : RestApiModel
//    {
//        public RestApiRoute<ICrudService<TodoItem>> Todos =>
//            RestApiRoute.Implementation.CrudOverDB<TodoDB>().Collection(db => db.Todos);
//    }
//
//    public class TodoDB : DatabaseModel
//    {
//        public DBCollection<TodoItem> Todos;
//    }
//
//    public class TodoPackage : Package<TodoEnvironmentConfig>
//    {
//        public TodoPackage(TodoEnvironmentConfig config) 
//            : base(config)
//        {
//        }
//            
//        protected override void Contribute(IContributions contributions)
//        {
//            contributions.IncludeDatabase<TodoDB>(() => new DatabaseConfig { 
//                ConnectionString = Config.DBConnectionString
//            });
//            
//            contributions.IncludeApiRoutes<TodoRestApi>(config: new ApiRoutesConfig {
//                BaseUrl = Config.RestApiUrl
//            });
//
//            contributions.IncludeWebApp<TodoWebApp>(config: new WebAppConfig {
//                BaseUrl = Config.WebAppUrl
//            });
//        }
//    }
//
//    public class TodoEnvironmentConfig
//    {
//        [Secret, Required]
//        public string DBConnectionString = null;
//        [Required]
//        public string WebAppUrl = null;
//        [Required]
//        public string RestApiUrl = null;
//    }
//
//    public class TodoService : Microservice<TodoEnvironmentConfig>
//    {
//        public TodoService(TodoEnvironmentConfig config) : base(
//            name: "todo-service", 
//            config, 
//            new MicroserviceOptions {
//                State = StateOption.Stateless,
//                Availability = AvailabilityOption.High2Nines  
//            })
//        {
//        }
//
//        protected override void Contribute(IContributions contributions)
//        {
//            contributions.IncludePackage(new TodoPackage(Config));
//        }
//    }
//}
