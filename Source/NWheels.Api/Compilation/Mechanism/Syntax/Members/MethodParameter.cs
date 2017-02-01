using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Syntax.Members
{
    public class MethodParameter
    {
        public MethodParameter()
        {
            this.Attributes = new List<AttributeDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodParameter(string name, int position, TypeMember type, MethodParameterModifier modifier, params AttributeDescription[] attributes)
            : this()
        {
            this.Name = name;
            this.Position = position;
            this.Type = type;
            this.Modifier = modifier;

            if (attributes != null)
            {
                this.Attributes.AddRange(attributes);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodParameter(string name, int position, TypeMember type)
            : this(name, position, type, MethodParameterModifier.None)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public int Position { get; set; }
        public TypeMember Type { get; set; }
        public MethodParameterModifier Modifier { get; set; }
        public List<AttributeDescription> Attributes { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodParameter ReturnVoid => null;
    }
}
