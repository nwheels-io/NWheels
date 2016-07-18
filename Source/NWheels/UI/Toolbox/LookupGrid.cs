using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Hapil;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "LookupGrid")]
    public class LookupGrid<TLookupId, TLookupRow> : DataGrid<TLookupRow>
        where TLookupRow : class
    {
        public LookupGrid(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "LookupGrid";
            this.TemplateName = "LookupGrid";
            this.Mode = DataGridMode.LookupMany;
            this.UsePascalCase = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<TLookupId[]> ModelSetter { get; set; }
    }
}
