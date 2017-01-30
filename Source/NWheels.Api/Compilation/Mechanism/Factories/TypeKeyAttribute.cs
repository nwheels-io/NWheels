using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum, 
        AllowMultiple = false, 
        Inherited = false)]
    public class TypeKeyAttribute : Attribute
    {
        public TypeKeyAttribute(
            Type factoryType, 
            Type primaryContract, 
            Type secondaryContract1,
            Type secondaryContract2,
            Type secondaryContract3,
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

        public Type FactoryType { get; }
        public Type PrimaryContract { get; }
        public Type SecondaryContract1 { get; }
        public Type SecondaryContract2 { get; }
        public Type SecondaryContract3 { get; }
        public int ExtensionValue1 { get; }
        public int ExtensionValue2 { get; }
        public int ExtensionValue3 { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey ToTypeKey()
        {
            return new TypeKey(
                FactoryType,
                PrimaryContract,
                SecondaryContract1,
                SecondaryContract2,
                SecondaryContract3,
                ExtensionValue1,
                ExtensionValue2,
                ExtensionValue3);
        }
    }
}
