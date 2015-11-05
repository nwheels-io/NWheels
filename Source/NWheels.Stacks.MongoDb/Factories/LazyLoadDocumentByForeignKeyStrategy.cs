using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Exceptions;
using TT = Hapil.TypeTemplate;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class LazyLoadDocumentByForeignKeyStrategy : LazyLoadByForeignKeyStrategyBase
    {
        private readonly MongoLazyLoadProxyFactory _lazyLoadProxyFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LazyLoadDocumentByForeignKeyStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty,
            MongoLazyLoadProxyFactory lazyLoadProxyFactory)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
            _lazyLoadProxyFactory = lazyLoadProxyFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            var lazyLoadProxyType = _lazyLoadProxyFactory.GetLazyLoadProxyType(RelatedContractType, RelatedImplementationType);

            using ( TT.CreateScope<TT.TKey, TT.TContract2, TT.TImpl2, TT.TServiceImpl>(
                ThisKeyProperty.ClrType,    // TT.TKey
                RelatedContractType,        // TT.TContract2 
                RelatedImplementationType,  // TT.TImpl2
                lazyLoadProxyType) )        // TT.TServiceImpl
            {
                writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                    getter: p => p.Get(m => {
                        base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;

                        m.If(StateField != DualValueStates.Contract).Then(() => {
                            ValueField.Assign(
                                m.New<TT.TServiceImpl>(
                                    m.This<TT.TBase>().Prop<TT.TKey>(ThisKeyProperty.ContractPropertyInfo)
                                )
                                .CastTo<TT.TProperty>()
                            );
                            StateField.Assign(DualValueStates.Contract);
                        });

                        m.Return(ValueField);
                    }),
                    setter: p => p.Set((m, value) => {
                        ValueField.Assign(value);
                        StateField.Assign(DualValueStates.Contract);
                    })
                );
            }
        }

        #endregion
    }
}
