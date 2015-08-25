using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class InteractiveUidlNode : AbstractUidlNode
    {
        protected InteractiveUidlNode(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
            : base(nodeType, idName, parent)
        {
            this.Notifications = new List<UidlNotification>();
            this.Text = base.IdName;
            this.Enabled = true;
            this.Authorization = new UidlAuthorization(GetAuthorizationAttributes());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IEnumerable<string> GetTranslatables()
        {
            return new[] {
                this.Text, this.HelpText, this.Icon
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string HelpText { get; set; }
        [DataMember]
        public string Icon { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public UidlAuthorization Authorization { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<UidlNotification> Notifications { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEnumerable<AuthorizationContract.AuthorizationAttribute> GetAuthorizationAttributes()
        {
            return this.GetType().GetCustomAttributes<AuthorizationContract.AuthorizationAttribute>(inherit: true);
        }
    }
}
