using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Notification", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlNotification : AbstractUidlNode
    {
        public UidlNotification(string idName, InteractiveUidlNode parent)
            : base(UidlNodeType.Notification, idName, parent)
        {
            this.SubscribedBehaviorQualifiedNames = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string PayloadType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<string> SubscribedBehaviorQualifiedNames { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class UidlNotification<TPayload> : UidlNotification, UidlBuilder.IBuildableUidlNode
    {
        public UidlNotification(string idName, InteractiveUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.Build(UidlBuilder builder)
        {
            base.PayloadType = builder.RegisterMetaType(typeof(TPayload));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.DescribePresenter(UidlBuilder builder)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        void UidlBuilder.IBuildableUidlNode.AttachExtensions(UidlExtensionRegistration[] registeredExtensions)
        {
        }
    }
}
