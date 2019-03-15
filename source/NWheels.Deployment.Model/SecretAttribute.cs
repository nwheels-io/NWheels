using System;

namespace NWheels.Deployment.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SecretAttribute : Attribute
    {
    }
}