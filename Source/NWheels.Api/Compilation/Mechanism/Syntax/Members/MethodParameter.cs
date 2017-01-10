using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class MethodParameter
    {
        public MethodParameter()
        {
            this.Attributes = new List<AttributeInfo>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public int Position { get; set; }
        public TypeMember Type { get; set; }
        public ParameterModifier Modifier { get; set; }
        public List<AttributeInfo> Attributes { get; private set; }
    }
}
