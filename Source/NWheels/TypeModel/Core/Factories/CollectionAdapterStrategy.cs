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
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using ProtoBuf.Meta;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class CollectionAdapterStrategy : PropertyImplementationStrategy
    {
        private Type _itemContractType;
        private Type _itemStorageType;
        private Field<ICollection<TT.TImpl>> _storageField;
        private Field<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>> _contractField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CollectionAdapterStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override bool OnShouldApply(IPropertyMetadata metaProperty)
        {
            return (
                metaProperty.Kind == PropertyKind.Relation &&
                metaProperty.Relation.Multiplicity.IsIn(RelationMultiplicity.OneToMany, RelationMultiplicity.ManyToMany));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override PropertyImplementationStrategy OnClone(IPropertyMetadata metaProperty)
        {
            return new CollectionAdapterStrategy(base.FactoryContext, base.MetadataCache, base.MetaType, metaProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBeforeImplementation(ClassWriterBase writer)
        {
            MetaProperty.ContractPropertyInfo.PropertyType.IsCollectionType(out _itemContractType);
            _itemStorageType = FindStorageType(_itemContractType);

            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemStorageType) )
            {
                _storageField = writer.Field<ICollection<TT.TImpl>>("psf_" + MetaProperty.Name);
                _contractField = writer.Field<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>("pcf_" + MetaProperty.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ClassWriterBase writer)
        {
            writer.ImplementInterfaceExplicitly<TT.TInterface>().Property(MetaProperty.ContractPropertyInfo).Implement(
                getter: p => p.Get(
                    m => m.Return(_contractField.CastTo<TT.TProperty>())
                )
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ClassWriterBase writer)
        {
            writer.ImplementBase<object>()
                .NewVirtualWritableProperty<ICollection<TT.TImpl>>(MetaProperty.Name).Implement(
                    getter: p => p.Get(m => {
                        m.Return(_storageField);
                    }),
                    setter: p => p.Set((m, value) => {
                        _contractField.Assign(m.New<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>(value));
                        _storageField.Assign(value);
                    }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, IOperand<IComponentContext> components)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemStorageType) )
            {
                _storageField.Assign(writer.New<HashSet<TT.TImpl>>());
                _contractField.Assign(writer.New<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>(_storageField));
            }
        }

        #endregion
    }
}
