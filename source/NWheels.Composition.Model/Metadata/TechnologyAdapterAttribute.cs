using System;

namespace NWheels.Composition.Model.Metadata
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TechnologyAdapterAttribute : Attribute
    {
        public TechnologyAdapterAttribute(Type adapterType)
        {
            AdapterType = adapterType;
        }

        public Type AdapterType { get; }
    }
}
