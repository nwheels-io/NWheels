using System;

namespace NWheels.Api
{
    public static class Guard
    {
        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class NotNullAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class NotEmptyAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class OrNullAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class PositiveAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class NotNegativeAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class GreaterThanAttribute : System.Attribute
        {
            public GreaterThanAttribute(object value)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public class GreaterOrEqualAttribute : System.Attribute
        {
            public GreaterOrEqualAttribute(object value)
            {
            }
        }
    }
}
