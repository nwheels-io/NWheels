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
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories.PropertyStrategies
{
    public class EmbeddedObjectPropertyStrategy : PropertyImplementationStrategy
    {
        private readonly DomainObjectFactoryContext _context;
        private Type _objectImplementationType;
        //private Field<TT.TProperty> _backingField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EmbeddedObjectPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap, 
            DomainObjectFactoryContext context, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, context.BaseContext, context.MetadataCache, context.MetaType, metaProperty)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            //_backingField = writer.Field<TT.TProperty>("m_" + MetaProperty.Name);
            _objectImplementationType = FindImplementationType(MetaProperty.ClrType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).ImplementAutomatic();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<object[]> valueVector)
        {
            //_context.DomainObjectFactoryField.Func<>()
            var field = HelpGetPropertyBackingField(writer);
            field.Assign(valueVector.ItemAt(MetaProperty.PropertyIndex).CastTo<TT.TProperty>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<object[]> valueVector)
        {
            var field = HelpGetPropertyBackingField(writer);
            valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(field.CastTo<object>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            if ( MetaProperty.Relation.RelatedPartyType.IsEntityPart || !MetaProperty.ContractPropertyInfo.CanWrite )
            {
                var field = HelpGetPropertyBackingField(writer);

                using ( TT.CreateScope<TT.TImpl>(_objectImplementationType) )
                {
                    field.Assign(writer.New<TT.TImpl>(components).CastTo<TT.TProperty>());
                }
            }
        }

        #endregion
    }
}
