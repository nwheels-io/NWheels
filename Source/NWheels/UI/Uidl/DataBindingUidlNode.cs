using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class DataBindingUidlNode : AbstractUidlNode
    {
        public DataBindingUidlNode(string idName, BindingSourceType sourceType, ControlledUidlNode parent)
            : base(UidlNodeType.DataBinding, idName, parent)
        {
            this.SourceType = sourceType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string BoundWidgetQualifiedName { get; set; }
        [DataMember]
        public string BoundWidgetPropertyExpression { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public BindingSourceType SourceType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<BindingTransformRule> TransformRules { get; private set; }
        [DataMember]
        public string TransformDefaultValueExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "TransformRule", Namespace = UidlDocument.DataContractNamespace)]
    public class BindingTransformRule
    {
        [DataMember]
        public string ConditionExpression { get; set; }
        [DataMember]
        public string ResultValueExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "ModelBinding", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlModelBinding : DataBindingUidlNode
    {
        public UidlModelBinding(string idName, BindingSourceType sourceType, ControlledUidlNode parent)
            : base(idName, sourceType, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string SourcePropertyExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "EntityBinding", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlEntityBinding : DataBindingUidlNode
    {
        public UidlEntityBinding(string idName, ControlledUidlNode parent)
            : base(idName, BindingSourceType.Entity, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityType { get; set; }
        [DataMember]
        public string QueryExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Name = "ApiBinding", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlApiBinding : DataBindingUidlNode
    {
        public UidlApiBinding(string idName, ControlledUidlNode parent)
            : base(idName, BindingSourceType.Api, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string ContractName { get; set; }
        [DataMember]
        public string OperationName { get; set; }
        [DataMember]
        public string[] ParameterExpressions { get; set; }
        [DataMember]
        public string ResultExpression { get; set; }
    }
}
