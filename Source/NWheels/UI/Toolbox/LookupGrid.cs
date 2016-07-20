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
    public class LookupGrid<TLookupId, TLookupRow> : DataGrid<TLookupRow>, IUidlLookupGrid
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

        public LookupGrid<TLookupId, TLookupRow> Filter<TInput>(Expression<Func<TInput, ViewModel<Empty.Data, Empty.State, TLookupRow>, bool>> expression)
        {
            QueryFilter = LookupDataFilter.DefineByExpression(expression);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<LookupDataFilter> QueryFilter { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<TLookupId[]> ModelSetter { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUidlLookupGrid
    {
        List<LookupDataFilter> QueryFilter { get; set; }
    }
}
