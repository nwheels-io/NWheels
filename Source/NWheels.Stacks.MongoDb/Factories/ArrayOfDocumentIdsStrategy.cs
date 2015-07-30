using System;
using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class ArrayOfDocumentIdsStrategy : DualValueStrategy
    {
        private Type _itemContractType;
        private Type _itemImplementationType;
        private Type _documentIdType;
        private Type _concreteCollectionType;
        private Field<IComponentContext> _componentsField;
        private IDisposable _typeTemplateScope;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ArrayOfDocumentIdsStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty, storageType: GetArrayOfIdsType(metaType))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            base.OnBeforeImplementation(writer);

            base.MetaProperty.ClrType.IsCollectionType(out _itemContractType);
            _itemImplementationType = FindImpementationType(_itemContractType);
            _documentIdType = MetaType.PrimaryKey.Properties[0].ClrType;
            _concreteCollectionType = HelpGetConcreteCollectionType(MetaProperty.ClrType, _itemContractType);

            _componentsField = writer.DependencyField<IComponentContext>("$components");
            
            _typeTemplateScope = TT.CreateScope<TT.TItem, TT.TImpl, TT.TKey, TT.TCollection<TT.TItem>>(
                _itemContractType, _itemImplementationType, _documentIdType, _concreteCollectionType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected override void OnAfterImplementation(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            _typeTemplateScope.Dispose();
            _typeTemplateScope = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components)
        {
            using ( TT.CreateScope<TT.TCollection<TT.TItem>>(_concreteCollectionType) )
            {
                base.ContractField.Assign(writer.New<TT.TCollection<TT.TItem>>().CastTo<TT.TProperty>());
                base.StateField.Assign(DualValueStates.Contract);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var m = writer;

            using ( TT.CreateScope<TT.TItem>(_itemContractType) )
            {
                m.If(base.StateField.EnumHasFlag(DualValueStates.Contract)).Then(() => {
                    nestedObjects.UnionWith(base.ContractField.CastTo<IEnumerable<TT.TItem>>().Cast<object>());

                    if ( typeof(IHaveNestedObjects).IsAssignableFrom(_itemImplementationType) )
                    {
                        m.ForeachElementIn(base.ContractField.CastTo<IEnumerable<TT.TItem>>().OfType<IHaveNestedObjects>()).Do((loop, item) => {
                            item.Void(x => x.DeepListNestedObjects, nestedObjects);
                        });
                    }
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasDependencies()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DualValueStrategy

        protected override void OnWritingContractToStorageConversion(
            MethodWriterBase method, 
            IOperand<TT.TProperty> contractValue, 
            MutableOperand<TT.TValue> storageValue)
        {
            var m = method; 

            var countLocal = m.Local<int>(initialValue: contractValue.CastTo<ICollection<TT.TItem>>().Count());
            var indexLocal = m.Local<int>(initialValueConst: 0);
            var idArrayLocal = m.Local<TT.TKey[]>(initialValue: m.NewArray<TT.TKey>(countLocal));

            m.ForeachElementIn(contractValue.CastTo<IEnumerable<TT.TItem>>().Cast<IEntityObject>()).Do((loop, item) => {
                idArrayLocal.ItemAt(indexLocal).Assign(item.Func<IEntityId>(x => x.GetId).Func<TT.TKey>(x => x.ValueAs<TT.TKey>));
                indexLocal.PostfixPlusPlus();
            });

            storageValue.Assign(idArrayLocal.CastTo<TT.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingStorageToContractConversion(
            MethodWriterBase method, 
            MutableOperand<TT.TProperty> contractValue, 
            IOperand<TT.TValue> storageValue)
        {
            var m = method;

            var lazyLoadQueryLocal = m.Local<IEnumerable<TT.TItem>>();
                
            lazyLoadQueryLocal.Assign(
                Static.Func(MongoDataRepositoryBase.ResolveFrom, _componentsField)
                .Func<IEnumerable<TT.TKey>, IEnumerable<TT.TItem>>(x => x.LazyLoadByIdList<TT.TItem, TT.TKey>, 
                    storageValue.CastTo<IEnumerable<TT.TKey>>()
                )                    
            );

            contractValue.Assign(m.New<TT.TCollection<TT.TItem>>(lazyLoadQueryLocal).CastTo<TT.TProperty>());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static Type GetArrayOfIdsType(ITypeMetadata metaType)
        {
            var idType = metaType.PrimaryKey.Properties[0].ClrType;
            return idType.MakeArrayType();
        }
    }
}
