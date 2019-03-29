using Demos.TodoList.Api;
using Demos.TodoList.Domain;
using NWheels.Composition.Model;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;
using NWheels.UI.Model.Web.Templates;
using NWheels.UI.RestApi.Model;

namespace Demos.TodoList.UI
{
    public class TodoListWebApp : SinglePageWebApp<TodoPage>
    {
    }

    public class TodoPage : SoloComponentPage
    {
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
            .WithDataSource(factory => factory.BackendRestApi<TodoListApi>().Route(api => api.TodoItem))
        );
    }
}
