using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public struct TypeKey : IEquatable<TypeKey>
    {
        public TypeKey(
            Type factoryType, 
            TypeMember primaryContract)
        {
            this.FactoryType = factoryType;
            this.PrimaryContract = primaryContract;
            this.SecondaryContract1 = null;
            this.SecondaryContract2 = null;
            this.SecondaryContract3 = null;
            this.ExtensionValue1 = 0;
            this.ExtensionValue2 = 0;
            this.ExtensionValue3 = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(
            Type factoryType, 
            TypeMember primaryContract, 
            TypeMember secondaryContract1, 
            int extensionValue1)
        {
            this.FactoryType = factoryType;
            this.PrimaryContract = primaryContract;
            this.SecondaryContract1 = secondaryContract1;
            this.SecondaryContract2 = null;
            this.SecondaryContract3 = null;
            this.ExtensionValue1 = extensionValue1;
            this.ExtensionValue2 = 0;
            this.ExtensionValue3 = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(
            Type factoryType, 
            TypeMember primaryContract, 
            TypeMember secondaryContract1, 
            TypeMember secondaryContract2, 
            int extensionValue1, 
            int extensionValue2)
        {
            this.FactoryType = factoryType;
            this.PrimaryContract = primaryContract;
            this.SecondaryContract1 = secondaryContract1;
            this.SecondaryContract2 = secondaryContract2;
            this.SecondaryContract3 = null;
            this.ExtensionValue1 = extensionValue1;
            this.ExtensionValue2 = extensionValue2;
            this.ExtensionValue3 = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(
            Type factoryType,
            TypeMember primaryContract,
            TypeMember secondaryContract1,
            TypeMember secondaryContract2,
            TypeMember secondaryContract3,
            int extensionValue1, 
            int extensionValue2,
            int extensionValue3)
        {
            this.FactoryType = factoryType;
            this.PrimaryContract = primaryContract;
            this.SecondaryContract1 = secondaryContract1;
            this.SecondaryContract2 = secondaryContract2;
            this.SecondaryContract3 = secondaryContract3;
            this.ExtensionValue1 = extensionValue1;
            this.ExtensionValue2 = extensionValue2;
            this.ExtensionValue3 = extensionValue3;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public readonly Type FactoryType;
        public readonly TypeMember PrimaryContract;
        public readonly TypeMember SecondaryContract1;
        public readonly TypeMember SecondaryContract2;
        public readonly TypeMember SecondaryContract3;
        public readonly int ExtensionValue1;
        public readonly int ExtensionValue2;
        public readonly int ExtensionValue3;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(TypeKey other)
        {
            if (this.FactoryType != other.FactoryType)
            {
                return false;
            }
            if (this.PrimaryContract != other.PrimaryContract)
            {
                return false;
            }
            if (this.ExtensionValue1 != other.ExtensionValue1)
            {
                return false;
            }
            if (this.SecondaryContract1 != other.SecondaryContract1)
            {
                return false;
            }
            if (this.ExtensionValue2 != other.ExtensionValue2)
            {
                return false;
            }
            if (this.SecondaryContract2 != other.SecondaryContract2)
            {
                return false;
            }
            if (this.ExtensionValue3 != other.ExtensionValue3)
            {
                return false;
            }
            if (this.SecondaryContract3 != other.SecondaryContract3)
            {
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(object obj)
        {
            if (obj is TypeKey other)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            int hashCode = FactoryType.GetHashCode();

            hashCode ^= (PrimaryContract != null ? 397 * PrimaryContract.GetHashCode() : 0);
            hashCode ^= ExtensionValue1 ^ (SecondaryContract1 != null ? 397 * SecondaryContract1.GetHashCode() : 0);
            hashCode ^= ExtensionValue2 ^ (SecondaryContract2 != null ? 397 * SecondaryContract2.GetHashCode() : 0);
            hashCode ^= ExtensionValue3 ^ (SecondaryContract3 != null ? 397 * SecondaryContract3.GetHashCode() : 0);

            return hashCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(TypeKey key1, TypeKey key2)
        {
            return key1.Equals(key2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator !=(TypeKey key1, TypeKey key2)
        {
            return !(key1 == key2);
        }
    }
}
