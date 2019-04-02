using System;

namespace NWheels.Composition.Model.Metadata
{
    [AttributeUsage(
        AttributeTargets.Enum | 
        AttributeTargets.Class | 
        AttributeTargets.Interface |
        AttributeTargets.Struct,
        Inherited = true)]
    public class ModelElementAttribute : Attribute
    {
        public Type Parser { get; set; }
    }
}
