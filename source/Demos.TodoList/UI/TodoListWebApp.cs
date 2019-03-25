using Demos.TodoList.Domain;
using NWheels.UI.Model;
using NWheels.UI.Model.Web;
using NWheels.UI.Model.Web.Templates;
using NWheels.UI.RestApi.Model;

namespace Demos.TodoList.UI
{
    public class TodoListWebApp : WebApp<Empty.Props, Empty.State>
    {
        [IndexPage]
        public TodoPage Todos => new TodoPage();
    }

    public class TodoPage : SoloComponentPage<Empty.Props, Empty.State>
    {
        public override UIComponent SoloComponent => new StackLayout(props => props
            .Row(NewTodoForm)
            .Row(TodoGrid)
        );
        
        Form<TodoItemEntity> NewTodoForm => new Form<TodoItemEntity>(props => props
            .WithFields(t => t.Title)
            .WithSubmitAction("Add")
        );
        
        DataGrid<TodoItemEntity> TodoGrid => new DataGrid<TodoItemEntity>(props => props
            .WithAutoColumns()
            .WithInlineEditor()
            .WithAppenderForm(NewTodoForm)
            .WithDataSource(factory => factory.BackendRestApiCrud())
        );
    }
}
