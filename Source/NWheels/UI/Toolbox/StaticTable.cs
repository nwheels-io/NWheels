using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "StaticTable")]
    public class StaticTable<TDataRow> : DataGrid<TDataRow>
    {
        public StaticTable(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "StaticTable";
            this.TemplateName = "StaticTable";
        }
    }
}
