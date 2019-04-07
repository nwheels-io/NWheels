using System;

namespace NWheels.Composition.Model.Impl.Metadata
{
    [AttributeUsage(
        AttributeTargets.Enum | 
        AttributeTargets.Class | 
        AttributeTargets.Interface |
        AttributeTargets.Struct,
        AllowMultiple = false,
        Inherited = true)]
    public class ModelParserAttribute : Attribute
    {
        public ModelParserAttribute(Type parser)
        {
            this.Parser = parser;
        }

        public Type Parser { get; }
    }
}
