using System;

namespace NWheels.Domain.Model
{
    public static class SemanticContract
    {
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class EmailAttribute : Attribute
        {
        }
        
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class UrlAttribute : Attribute
        {
        }
        
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class ConnectionStringAttribute : Attribute
        {
        }
    }
}
