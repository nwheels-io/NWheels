using System;
using System.Reflection;

namespace NWheels.DataObjects
{
    public interface IDomainContextEntityMetadata
    {
        PropertyInfo RepositoryProperty { get; }
        PropertyInfo PartitionedRepositoryProperty { get; }
        ITypeMetadata MetaType { get; }
        Type ContractType { get; }
        Type ImplementationType { get; }
    }
}
