using System;

namespace NWheels.Domain.Model
{
    public class ValueContract
    {
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class RequiredAttribute : Attribute
        {
        }
    
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class UniqueAttribute : Attribute
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
        public class AutoGeneratedAttribute : Attribute
        {
            public AutoGeneratedAttribute(long initialValue = 1)
            {
                InitialValue = initialValue;
            }

            public readonly long InitialValue;
        }
    }
}