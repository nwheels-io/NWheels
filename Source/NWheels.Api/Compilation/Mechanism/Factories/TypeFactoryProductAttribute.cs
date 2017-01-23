using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum, 
        AllowMultiple = false, 
        Inherited = false)]
    public class TypeFactoryProductAttribute : Attribute
    {
        private readonly Type[] _secondaryContracts;
        private readonly object[] _extensionValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeFactoryProductAttribute(Type factory, Type primaryContract, Type[] secondaryContracts, object[] extensionValues)
        {
            this.Factory = factory;
            this.PrimaryContract = primaryContract;

            _secondaryContracts = secondaryContracts;
            _extensionValues = extensionValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type Factory { get; }
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
