using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public abstract class AbstractUidlNode
    {
        /// <summary>
        /// Make IdName equal to current type name
        /// </summary>
        public const string IdNameAsTypeMacro = "${TYPE}";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadataCache GetMetadataCache()
        {
            return this.MetadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractUidlNode(UidlNodeType nodeType, string idName, AbstractUidlNode parent)
        {
            this.NodeType = nodeType;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            this.IdName = (idName == IdNameAsTypeMacro ? GetIdNameFromType() : idName);
            this.QualifiedName = (parent != null ? parent.QualifiedName + ":" : "") + IdName;
            this.ElementName = (parent != null ? parent.ElementName + "-" : "") + IdName.Replace("<", "").Replace(">", "");

            if ( parent != null )
            {
                this.MetadataCache = parent.MetadataCache;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public UidlNodeType NodeType { get; set; }
        [DataMember]
        public string IdName { get; set; }
        [DataMember]
        public string QualifiedName { get; set; }
        [DataMember]
        public string ElementName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string GetIdNameFromType()
        {
            return this.GetType().FriendlyName();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual void OnDeclaredMemberNodeCreated(PropertyInfo declaration, AbstractUidlNode instance)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ITypeMetadataCache MetadataCache { get; set; }
    }
}
