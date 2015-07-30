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
using NWheels.DataObjects.Core.StorageTypes;
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

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            MetaProperty.ContractPropertyInfo.PropertyType.IsCollectionType(out _itemContractType);
            _itemStorageType = FindImpementationType(_itemContractType);

            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemStorageType) )
            {
                _storageField = writer.Field<ICollection<TT.TImpl>>("m_" + MetaProperty.Name + "$storage");
                _contractField = writer.Field<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>("m_" + MetaProperty.Name + "$adapter");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                getter: p => p.Get(
                    m => m.Return(_contractField.CastTo<TT.TProperty>())
                )
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl>(_itemContractType, _itemStorageType) )
            {
                writer.ImplementBase<object>().NewVirtualWritableProperty<ICollection<TT.TImpl>>(MetaProperty.Name).Implement(
                    getter: p => p.Get(m => {
                        m.Return(_storageField);
                    }),
                    setter: p => p.Set((m, value) => {
                        _contractField.Assign(m.New<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>(value));
                        _storageField.Assign(value);
                    }));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components)
        {
            var concreteCollectionType = HelpGetConcreteCollectionType(MetaProperty.ClrType, _itemStorageType); 

            using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TCollection<TT.TImpl>>(_itemContractType, _itemStorageType, concreteCollectionType) )
            {
                _storageField.Assign(writer.New<TT.TCollection<TT.TImpl>>());
                _contractField.Assign(writer.New<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>(_storageField));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var m = writer;

            using ( TT.CreateScope<TT.TItem>(_itemContractType) )
            {
                nestedObjects.UnionWith(_contractField.CastTo<IEnumerable<TT.TItem>>().Cast<object>());

                if ( typeof(IHaveNestedObjects).IsAssignableFrom(_itemStorageType) )
                {
                    m.ForeachElementIn(_contractField.CastTo<IEnumerable<TT.TItem>>().OfType<IHaveNestedObjects>()).Do((loop, item) => {
                        item.Void(x => x.DeepListNestedObjects, nestedObjects);
                    });
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        #endregion
    }
}
