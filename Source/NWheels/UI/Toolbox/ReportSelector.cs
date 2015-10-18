using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class ReportSelector : WidgetBase<ReportSelector, Empty.Data, Empty.State>
    {
        public ReportSelector(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetCriteriaModel<TCriteria>()
        {
            var baseMetaType = GetMetadataCache().GetTypeMetadata(typeof(TCriteria));
            this.ReportTypes = (TypeSelector)UidlUtility.CreateFormOrTypeSelector(baseMetaType, "ReportTypes", this, isInline: true);

            foreach ( var selection in ReportTypes.Selections )
            {
                var concreteReport = new Report("Report_" + selection.MetaType.Name, this);
                concreteReport.CriteriaForm = selection.Widget;

                //selection.MetaType.ContractType
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public TypeSelector ReportTypes { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ReportSelector, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().Concat(new[] { ReportTypes });
        }

        #endregion
    }
}
