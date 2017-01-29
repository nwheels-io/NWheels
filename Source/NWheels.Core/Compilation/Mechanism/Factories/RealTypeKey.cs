using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation.Mechanism.Factories
{
    public class RealTypeKey<TExtension> : TypeKey<TExtension>, ITypeKeyInternals, IEquatable<TypeKey>, IEquatable<TypeKey<TExtension>>
        where TExtension : ITypeKeyExtension, new()
    {
        private readonly int _hashCode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RealTypeKey(Type factoryType, TypeMember primaryContract, TypeMember[] secondaryContracts, TExtension extension)
        {
            this.FactoryType = factoryType;
            this.PrimaryContract = primaryContract;
            this.SecondaryContracts = secondaryContracts;
            this.Extension = extension;

            _hashCode = CalculateHashCode();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeFactoryContext ITypeKeyInternals.CreateContext<TContextExtension>(TypeMember product, TContextExtension contextExtension)
        {
            return new TypeFactoryContext<TExtension, TContextExtension>(this, product, contextExtension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(TypeKey<TExtension> other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.FactoryType != this.FactoryType)
            {
                return false;
            }

            if (other.PrimaryContract != this.PrimaryContract)
            {
                return false;
            }

            if (this.SecondaryContracts != null)
            {
                if (other.SecondaryContracts == null)
                {
                    return false;
                }

                if (other.SecondaryContracts.Count != this.SecondaryContracts.Count)
                {
                    return false;
                }

                for (int i = 0 ; i < this.SecondaryContracts.Count ; i++)
                {
                    if (other.SecondaryContracts[i] != this.SecondaryContracts[i])
                    {
                        return false;
                    }
                }
            }
            else if (other.SecondaryContracts != null)
            {
                return false;
            }

            if (typeof(TExtension) == typeof(Empty.KeyExtension))
            {
                return true;
            }

            if (this.Extension != null)
            {
                if (other.Extension == null)
                {
                    return false;
                }

                if (!other.Extension.Equals(this.Extension))
                {
                    return false;
                }
            }
            else if (other.Extension != null)
            {
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(TypeKey obj)
        {
            if (obj is TypeKey<TExtension> other)
            {
                return Equals(other);
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if (obj is TypeKey<TExtension> other)
            {
                return Equals(other);
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode() => _hashCode;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type FactoryType { get; }
        public override TypeMember PrimaryContract { get; }
        public override IReadOnlyList<TypeMember> SecondaryContracts { get; }
        public override TExtension Extension { get; }
        public override object ExtensionObject => this.Extension;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type ExtensionType => typeof(TExtension);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int CalculateHashCode()
        {
            var result = (PrimaryContract != null ? PrimaryContract.GetHashCode() : 17);

            if (SecondaryContracts != null)
            {
                result ^= SecondaryContracts.Count;

                for (int i = 0 ; i < SecondaryContracts.Count ; i++)
                {
                    result ^= SecondaryContracts[i].GetHashCode();
                }
            }

            if (Extension != null)
            {
                result ^= Extension.GetHashCode();
            }

            return result;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITypeKeyInternals
    {
        ITypeFactoryContext CreateContext<TContextExtension>(TypeMember product, TContextExtension contextExtension);
    }
}
