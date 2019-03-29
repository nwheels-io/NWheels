using Demos.TodoList.Api;
using Demos.TodoList.Domain;
using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;
using NWheels.UI.Model.Web.Templates;
using NWheels.UI.RestApi.Model;

namespace Demos.TodoList.UI
{
    public class TodoListWebApp : WebApp<Empty.Props, Empty.State>
    {
        private readonly TodoListApi _backendApi;

        public TodoListWebApp(TodoListApi backendApi)
        {
            _backendApi = backendApi;
        }
        
        [Include]
        TodoPage Index => new TodoPage(_backendApi);
    }

    public class TodoPage : SoloComponentPage
    {
        public TodoPage(TodoListApi backendApi)
        {
            this.BackendApi = backendApi;
        }
        
        private TodoListApi BackendApi { get; }
        
        public override UIComponent SoloComponent => new StackLayout(props => props
            .Row(NewTodoForm)
            .Row(TodoGrid)
        );
        
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
            .WithDataSource(BackendApi.TodoItems.AsDataSource())
        );
    }
}
