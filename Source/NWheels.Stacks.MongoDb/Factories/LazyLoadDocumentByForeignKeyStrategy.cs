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
        public LazyLoadDocumentByForeignKeyStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TKey, TT.TContract2, TT.TImpl2>(ThisKeyProperty.ClrType, RelatedContractType, RelatedImplementationType) )
            {
                writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                    getter: p => p.Get(m => {
                        base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;

                        m.If(StateField != DualValueStates.Contract).Then(() => {
                            ValueField.Assign(
                                Static.Func(MongoDataRepositoryBase.ResolveFrom, ComponentsField)
                                .Func<string, TT.TKey, TT.TContract2>(
                                    x => x.LazyLoadOneByForeignKey<TT.TContract2, TT.TImpl2, TT.TKey>, 
                                    m.Const(ForeignKeyProperty.Name),
                                    m.This<TT.TBase>().Prop<TT.TKey>(ThisKeyProperty.ContractPropertyInfo))
                                .CastTo<TT.TProperty>());

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
