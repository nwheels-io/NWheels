using System;
using System.Collections.Generic;
using System.Text;
using NWheels.UI;
using NWheels.UI.Components;
using NWheels.UI.Web;

namespace TodoListApp
{
    public class TodoWebApp : WebApp<Empty.Model, Empty.Args>
    {
        [NWheels.UI.TypeContract.TemplateUrl("theme://page?type=big-front")]
        public class IndexPage : WebPage<Empty.Model, Empty.Args>
        {
            [CrudComponent.Configure(DataOption = CrudDataOption.Repository)]
            [DataGridComponent.Configure(RowReOrder = RowReOrderOptions.DragDrop)]
            public CrudComponent<TodoItem> Crud { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void Configuraiton()
            {
                Crud.Grid.Columns.Configure(x => x.Description, relativeWidth: 10);
                Crud.Grid.Rows.StyleIf(x => x.Done, style => style.Font.StrikeThrough());
            }
        }
    }
}
