using System;

namespace NWheels.Api.Uidl
{
    public static class UIModel
    {
        [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
        public class InjectedDependencyAttribute : System.Attribute
        {
        }
    }
}
