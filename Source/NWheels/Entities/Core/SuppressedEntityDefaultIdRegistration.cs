using System;

namespace NWheels.Entities.Core
{
    public class SuppressedEntityDefaultIdRegistration
    {
        public SuppressedEntityDefaultIdRegistration(Type entityContractType)
        {
            EntityContractType = entityContractType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EntityContractType { get; private set; }
    }
}