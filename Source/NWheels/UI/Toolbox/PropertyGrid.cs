using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "PropertyGrid")]
    public class PropertyGrid<TEntity> : Form<TEntity>
        where TEntity : class
    {
        public PropertyGrid(string idName, ControlledUidlNode parent)
            : this(idName, parent, isNested: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyGrid(string idName, ControlledUidlNode parent, bool isNested)
            : base(idName, parent, isNested)
        {
            this.WidgetType = this.TemplateName = "PropertyGrid";
        }
    }
}
