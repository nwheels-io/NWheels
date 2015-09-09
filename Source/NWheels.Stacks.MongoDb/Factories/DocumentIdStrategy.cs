using System;
using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using MongoDB.Bson;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class DocumentIdStrategy : DualValueStrategy
    {
        private Field<IComponentContext> _componentsField;
        private Type _implementationType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DocumentIdStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty, storageType: metaProperty.Relation.RelatedPartyType.PrimaryKey.Properties[0].ClrType)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            base.OnBeforeImplementation(writer);

            _implementationType = FindImpementationType(MetaProperty.ClrType);
            _componentsField = writer.DependencyField<IComponentContext>("$components");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var m = writer;

            m.If(base.StateField.EnumHasFlag(DualValueStates.Contract)).Then(() => {
                Static.Void(RuntimeTypeModelHelpers.DeepListNestedObject, base.ContractField, nestedObjects);
                //nestedObjects.Add(base.ContractField);

                //if ( typeof(IHaveNestedObjects).IsAssignableFrom(_implementationType) )
                //{
                //    base.ContractField.CastTo<IHaveNestedObjects>().Void(x => x.DeepListNestedObjects, nestedObjects);
                //}
            });
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
            method.If(contractValue.CastTo<IEntityObject>().IsNotNull()).Then(() => {
                storageValue.Assign(contractValue.CastTo<IEntityObject>().Func<IEntityId>(x => x.GetId).Func<TT.TValue>(x => x.ValueAs<TT.TValue>));
            }).Else(() => {
                storageValue.Assign(Static.Prop(() => ObjectId.Empty).CastTo<TT.TValue>());
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingStorageToContractConversion(
            MethodWriterBase method, 
            MutableOperand<TT.TProperty> contractValue, 
            IOperand<TT.TValue> storageValue)
        {
            contractValue.Assign(
                Static.Func(MongoDataRepositoryBase.ResolveFrom, _componentsField)
                    .Func<TT.TValue, TT.TProperty>(x => x.LazyLoadById<TT.TProperty, TT.TValue>, storageValue));
        }

        #endregion
    }
}
