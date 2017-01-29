using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class TypeKey : IEquatable<TypeKey>
    {
        public abstract Type FactoryType { get; }
        public abstract TypeMember PrimaryContract { get; }
        public abstract IReadOnlyList<TypeMember> SecondaryContracts { get; }
        public abstract Type ExtensionType { get; }
        public abstract object ExtensionObject { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract bool Equals(TypeKey other);
        public override bool Equals(object obj) => throw new NotImplementedException("To be overridden by concrete implementation");
        public override int GetHashCode() => throw new NotImplementedException("To be overridden by concrete implementation");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(TypeKey key1, TypeKey key2)
        {
            if (!ReferenceEquals(key1, null))
            {
                return key1.Equals(key2);
            }

            return ReferenceEquals(key2, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator !=(TypeKey key1, TypeKey key2)
        {
            return !(key1 == key2);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TypeKey<TExtension> : TypeKey
        where TExtension : ITypeKeyExtension, new()
    {
        public abstract TExtension Extension { get; }
    }
}
