using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class LazyLoadCollectionByForeignKeyStrategy : LazyLoadByForeignKeyStrategyBase
    {
        public LazyLoadCollectionByForeignKeyStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            var collectionObjectType = HelpGetConcreteCollectionType(MetaProperty.ClrType, RelatedContractType);

            using ( TT.CreateScope<TT.TKey, TT.TContract2, TT.TImpl2, TT.TAbstractCollection<TT.TContract2>>(
                ThisKeyProperty.ClrType, RelatedContractType, RelatedImplementationType, collectionObjectType) )
            {
                writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                    getter: p => p.Get(m => {
                        base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;

                        m.If(StateField != DualValueStates.Contract).Then(() => {
                            var enumerableLocal = m.Local<IEnumerable<TT.TContract2>>();
                            
                            enumerableLocal.Assign(
                                Static.Func(MongoDataRepositoryBase.ResolveFrom, 
                                    ComponentsField,
                                    Static.Func(ResolutionExtensions.Resolve<DataRepositoryBase>, ComponentsField).Func<Type>(x => x.GetType)
                                )
                                .Func<string, TT.TKey, IEnumerable<TT.TContract2>>(x => x.LazyLoadManyByForeignKey<TT.TContract2, TT.TImpl2, TT.TKey>,
                                    m.Const(ForeignKeyProperty.Name),
                                    m.This<TT.TBase>().Prop<TT.TKey>(ThisKeyProperty.ContractPropertyInfo)));

                            var collectionLocal = m.Local(initialValue: m.New<TT.TAbstractCollection<TT.TContract2>>(enumerableLocal));
                            
                            ValueField.Assign(collectionLocal.CastTo<TT.TProperty>());
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
    }
}
