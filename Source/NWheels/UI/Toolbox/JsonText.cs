using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class JsonText : WidgetBase<JsonText, Empty.Data, Empty.State>
    {
        public JsonText(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public bool ExpandedByDefault { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<JsonText, Empty.Data, Empty.State> presenter)
        {
        }
    }
}
