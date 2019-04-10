using Demos.TodoList.Api;
using Demos.TodoList.DevOps;
using Demos.TodoList.Domain;
using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;
using NWheels.UI.Model.Web.Templates;
using NWheels.UI.RestApi.Model;

namespace Demos.TodoList.FrontEnd
{
    public class TodoListWebApp : WebApp<TodoListUrlsConfig, Empty.State>
    {
        public TodoListWebApp(TodoListUrlsConfig props) : base(props)
        {
        }

        TodoPage Index => new TodoPage(Props);
    }

    public class TodoListApiProxy : BackendApiProxy<TodoListApi>
    {
        public TodoListApiProxy(string url) : base(url)
        {
        }
    }
    
    public class TodoPage : SoloComponentPage<TodoListUrlsConfig, Empty.State>
    {
        public TodoPage(TodoListUrlsConfig props) : base(props)
        {
        }

        public override string Title => "Demo Todo List";

        public override UIComponent SoloComponent => new StackLayout(props => props
            .Row(NewTodoForm)
            .Row(TodoGrid)
        );
     
        [Include]
        TodoListApiProxy Backend => new TodoListApiProxy(Props.BackendApiUrl);

        [Include]
        Form<TodoItemEntity> NewTodoForm => new Form<TodoItemEntity>(props => props
            .WithFields(t => t.Title)
            .WithSubmitAction("Add")
        );
        
        [Include]
        DataGrid<TodoItemEntity> TodoGrid => new DataGrid<TodoItemEntity>(props => props
            .WithAutoColumns()
            .WithInlineEditor()
            .WithAppenderForm(NewTodoForm)
            .WithDataSource(Backend.Api.TodoItems.AsDataSource())
        );
    }
}
