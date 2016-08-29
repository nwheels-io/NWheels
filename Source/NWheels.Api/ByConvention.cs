using System;

namespace NWheels.Api
{
    public static class ByConvention
    {
        [AttributeUsage(AttributeTargets.Interface)]
        public class LocalizableResourcesAttribute : System.Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Interface)]
        public class SemanticLoggerAttribute : System.Attribute
        {
        }
    }
}
