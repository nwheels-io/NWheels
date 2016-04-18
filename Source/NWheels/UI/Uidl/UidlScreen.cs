using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Screen", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlScreen : RootContentUidlNode
    {
        public UidlScreen(string idName, UidlApplication parent)
            : base(UidlNodeType.Screen, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public ScreenKind ScreenKind { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ScreenKind
    {
        Unspecified,
        LandingPage,
        PublicPage,
        SignInSignUp,
        DashboardAdmin
    }
}
