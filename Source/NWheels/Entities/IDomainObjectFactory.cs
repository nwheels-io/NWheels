using System;

namespace NWheels.Entities
{
    public interface IDomainObjectFactory
    {
        Type GetOrBuildDomainObjectType(Type contractType, Type persistableFactoryType);
        TEntityContract CreateDomainObjectInstance<TEntityContract>(TEntityContract underlyingPersistableObject);
    }
}