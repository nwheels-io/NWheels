using NWheels.DataObjects.Core;

namespace NWheels.DataObjects.Core
{
    public interface IObjectContractAttribute
    {
        void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache);
    }
}