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
        private readonly Type[] _secondaryContracts;
        private readonly object[] _extensionValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKeyAttribute(Type factoryType, Type primaryContract, Type[] secondaryContracts, Type extensionType, object[] extensionValues)
        {
            this.FactoryType = factoryType;
            this.PrimaryContract = primaryContract;
            this.ExtensionType = extensionType;

            _secondaryContracts = secondaryContracts;
            _extensionValues = extensionValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type FactoryType { get; }
        public Type ExtensionType { get; }
        public Type PrimaryContract { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<Type> SecondaryContracts => _secondaryContracts;
        public IReadOnlyList<object> ExtensionValues => _extensionValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TExtension DeserializeTypeKeyExtension<TExtension>()
            where TExtension : ITypeKeyExtension, new()
        {
            var extension = new TExtension();
            extension.Deserialize(_extensionValues);
            return extension;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object[] SerializeTypeKeyExtension(ITypeKeyExtension extension)
        {
            return extension?.Serialize();
        }
    }
}
