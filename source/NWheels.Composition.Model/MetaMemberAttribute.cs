using System;

namespace NWheels.Composition.Model
{
    [AttributeUsage(
        AttributeTargets.Class | 
        AttributeTargets.Interface | 
        AttributeTargets.Method | 
        AttributeTargets.Property | 
        AttributeTargets.Field 
    )]
    public class MetaMemberAttribute : Attribute
    {
    }
}