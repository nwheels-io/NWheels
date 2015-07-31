using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;
using NWheels.DataObjects.Core;

namespace NWheels.TypeModel.Core.Factories
{
    public class PublicAccessorWrapperStrategy : PropertyImplementationStrategy
    {
        private Field<TT.TProperty> _storageField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PublicAccessorWrapperStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            _storageField = writer.Field<TT.TProperty>("m_" + MetaProperty.Name + "$storage");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            var canRead = MetaProperty.ContractPropertyInfo.CanRead;
            var canWrite = MetaProperty.ContractPropertyInfo.CanWrite;

            writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                getter: p => canRead ? p.Get(m => m.Return(_storageField)) : null,
                setter: p => canWrite ? p.Set((m, value) => _storageField.Assign(value)) : null
            );

            writer.OwnerClass.SetPropertyBackingField(base.MetaProperty.ContractPropertyInfo, _storageField);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            writer.ImplementBase<object>().NewVirtualWritableProperty<TT.TProperty>(MetaProperty.Name).ImplementAutomatic(_storageField);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components)
        {
            HelpInitializeDefaultValue(writer, components);
        }

        #endregion
    }
}
