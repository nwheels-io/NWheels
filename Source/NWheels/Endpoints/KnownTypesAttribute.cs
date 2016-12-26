using System;

namespace NWheels.Endpoints
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class KnownTypesAttribute : Attribute
    {
        public KnownTypesAttribute(params Type[] types)
        {
            this.Types = types;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type[] Types { get; private set; }
    }
}