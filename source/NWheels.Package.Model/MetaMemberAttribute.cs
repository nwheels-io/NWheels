using System;

namespace NWheels.Package.Model
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