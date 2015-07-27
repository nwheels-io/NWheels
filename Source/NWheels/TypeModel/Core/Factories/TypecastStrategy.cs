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

namespace NWheels.TypeModel.Core.Factories
{
    public class TypecastStrategy : PropertyImplementationStrategy
    {
        private Type _storageType;
        private Field<TT.TValue> _storageField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypecastStrategy(ObjectFactoryContext factoryContext, ITypeMetadataCache metadataCache, ITypeMetadata metaType, IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override bool OnShouldApply(IPropertyMetadata metaProperty)
        {
            return (metaProperty.Kind.IsIn(PropertyKind.Relation, PropertyKind.Part) && !metaProperty.ClrType.IsCollectionType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override PropertyImplementationStrategy OnClone(IPropertyMetadata metaProperty)
        {
            return new TypecastStrategy(base.FactoryContext, base.MetadataCache, base.MetaType, metaProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBeforeImplementation(ClassWriterBase writer)
        {
            _storageType = FindStorageType(contractType: base.MetaProperty.ContractPropertyInfo.PropertyType);

            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                _storageField = writer.Field<TT.TValue>("psf_" + MetaProperty.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ClassWriterBase writer)
        {
            var canRead = MetaProperty.ContractPropertyInfo.CanRead;
            var canWrite = MetaProperty.ContractPropertyInfo.CanWrite;

            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                writer.ImplementInterfaceExplicitly<TT.TInterface>()
                    .Property(MetaProperty.ContractPropertyInfo).Implement(
                        getter: p => canRead ? p.Get(m => m.Return(_storageField.CastTo<TT.TProperty>())) : null,
                        setter: p => canWrite ? p.Set((m, value) => _storageField.Assign(value.CastTo<TT.TValue>())) : null
                    );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ClassWriterBase writer)
        {
            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                writer.ImplementBase<object>().NewVirtualWritableProperty<TT.TValue>(MetaProperty.Name).ImplementAutomatic(_storageField);
            }
        }

        #endregion
    }
}
