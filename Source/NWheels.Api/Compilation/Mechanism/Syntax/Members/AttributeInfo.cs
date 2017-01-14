using NWheels.Compilation.Mechanism.Syntax.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class AttributeInfo
    {
        public AttributeInfo()
        {
            this.ConstructorArguments = new List<AbstractExpression>();
            this.PropertyValues = new List<AttributePropertyValueInfo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMember AttributeType { get; set; }
        public List<AbstractExpression> ConstructorArguments { get; private set; }
        public List<AttributePropertyValueInfo> PropertyValues { get; private set; }
        public Attribute Binding { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class AttributePropertyValueInfo
    {
        public string Name { get; set; }
        public AbstractExpression Value { get; set; }
    }
}
