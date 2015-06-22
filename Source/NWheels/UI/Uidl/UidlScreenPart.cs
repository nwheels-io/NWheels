using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "ScreenPart", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlScreenPart : RootContentUidlNode
    {
        public UidlScreenPart(string idName, UidlApplication parent)
            : base(UidlNodeType.ScreenPart, idName, parent)
        {
        }
    }
}
