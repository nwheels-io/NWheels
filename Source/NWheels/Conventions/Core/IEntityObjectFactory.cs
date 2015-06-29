using System;
using NWheels.DataObjects.Core;

namespace NWheels.Conventions.Core
{
    public interface IEntityObjectFactory
    {
        TEntityContract NewEntity<TEntityContract>() where TEntityContract : class;
        object NewEntity(Type entityContractType);
    }
}