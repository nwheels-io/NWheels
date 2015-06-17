using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class DataBindingDescription : UINodeDescription
    {
        protected DataBindingDescription(string idName, BindingSourceType sourceType, PresenterDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.DataBinding;
            this.SourceType = sourceType;
            this.TransformRules = new List<BindingTransformRule>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BindingSourceType SourceType { get; private set; }
        public string TargetPropertyExpression { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<BindingTransformRule> TransformRules { get; private set; }
        public string TransformDefaultValueExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BindingSourceType
    {
        Data,
        State,
        Api,
        Entity
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class BindingTransformRule
    {
        public string ConditionExpression { get; set; }
        public string ResultValueExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ModelDataBinding : DataBindingDescription
    {
        public ModelDataBinding(string idName, BindingSourceType sourceType, PresenterDescription parent)
            : base(idName, sourceType, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SourcePropertyExpression { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityDataBinding : DataBindingDescription
    {
        public EntityDataBinding(string idName, PresenterDescription parent)
            : base(idName, BindingSourceType.Entity, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string EntityType { get; set; }
        public string RestQuery { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ApiDataBinding : DataBindingDescription
    {
        public ApiDataBinding(string idName, PresenterDescription parent)
            : base(idName, BindingSourceType.Api, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContractName { get; set; }
        public string OperationName { get; set; }
        public string[] ParameterExpressions { get; set; }
        public string ValueExpression { get; set; }
    }
}
