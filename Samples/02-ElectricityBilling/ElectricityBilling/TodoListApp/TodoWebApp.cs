using System;
using System.Linq;
using NWheels.UI;
using NWheels.UI.Components;
using NWheels.UI.Web;

namespace TodoListApp
{
    public class TodoWebApp : WebApp<Empty.Model, Empty.Args>
    {
        [NWheels.UI.TypeContract.TemplateUrl("theme://page?type=big-front")]
        public class IndexPage : WebPage<IndexPage.TodoListModel, Empty.Args>
        {
            public ToolbarComponent Toolbar { get; }

            [DataSourceComponent.Configure(DataOption = DataSourceOption.Repository)]
            [DataGridComponent.Configure(RowReOrder = RowReOrderOptions.DragDrop)]
            public CrudComponent<TodoItem> Crud { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void Configuration()
            {
                Toolbar.Structure = new {
                    Show = Structure.Builder.ToggleGroup<bool?>(
                        new {
                            All = (bool?)null,
                            Active = false,
                            Completed = true
                        }, 
                        onChange: value => Model.DoneOption = value
                    )
                };

                Bind(Crud.Data.Query).To(m => m.DoneOption, 
                    transform: doneOption => Crud.Data.Source.Where(
                        x => doneOption == null || x.Done == doneOption));

                Crud.Grid.Columns.Configure(x => x.Description, relativeWidth: 10);
                Crud.Grid.Rows.StyleIf(x => x.Done, style => style.Class("done"));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class TodoListModel
            {
                public bool? DoneOption { get; set; }
            }
        }
    }
}
