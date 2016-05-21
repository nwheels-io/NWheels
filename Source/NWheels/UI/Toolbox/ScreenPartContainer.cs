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
    public class ScreenPartContainer : WidgetUidlNode
    {
        public ScreenPartContainer(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetInitialScreenPart<TInput>(IScreenPartWithInput<TInput> screenPart)
        {
            this.InitalScreenPartQualifiedName = screenPart.QualifiedName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string InitalScreenPartQualifiedName { get; set; }
    }
}
