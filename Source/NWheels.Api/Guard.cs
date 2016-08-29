using System;

namespace NWheels.Api
{
    public static class Guard
    {
        [AttributeUsage(AttributeTargets.Parameter)]
        public class NotNullAttribute : System.Attribute
        {
        }
    }
}
