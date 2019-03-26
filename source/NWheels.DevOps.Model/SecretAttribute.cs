using System;

namespace NWheels.DevOps.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SecretAttribute : Attribute
    {
    }
}
