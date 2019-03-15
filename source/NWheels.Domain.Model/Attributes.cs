using System;

namespace NWheels.Domain.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RequiredAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MaxLengthAttribute : Attribute
    {
        public MaxLengthAttribute(int length)
        {
            this.Length = length;
        }
        
        public int Length { get; }
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EmailSemanticsAttribute : Attribute
    {
    }
}
