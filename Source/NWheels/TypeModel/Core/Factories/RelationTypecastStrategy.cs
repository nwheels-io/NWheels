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
    public class RelationTypecastStrategy : PropertyImplementationStrategy
    {
        private Type _storageType;
        private Field<TT.TValue> _storageField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RelationTypecastStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            _storageType = FindImplementationType(contractType: base.MetaProperty.ContractPropertyInfo.PropertyType);

            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                _storageField = writer.Field<TT.TValue>("m_" + MetaProperty.Name + "$storage");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            var canRead = MetaProperty.ContractPropertyInfo.CanRead;
            var canWrite = MetaProperty.ContractPropertyInfo.CanWrite;

            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                    getter: p => 
                        canRead 
                        ? p.Get(m => {
                            base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                            m.Return(_storageField.CastTo<TT.TProperty>());
                        }) 
                        : null,
                    setter: p => 
                        canWrite 
                        ? p.Set((m, value) => {
                            base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                            _storageField.Assign(value.CastTo<TT.TValue>());
                        }) 
                        : null
                );

                writer.OwnerClass.SetPropertyBackingField(base.MetaProperty.ContractPropertyInfo, _storageField);
                WriteComplementContractAccessorMethods(writer, canRead, canWrite);
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteComplementContractAccessorMethods(ImplementationClassWriter<TypeTemplate.TInterface> writer, bool canRead, bool canWrite)
        {
            if ( !canRead )
            {
                writer.NewVirtualFunction<TT.TProperty>(GetReadAccessorMethodName(MetaProperty))
                    .Implement(w => w.Return(_storageField.CastTo<TT.TProperty>()));
            }

            if ( !canWrite )
            {
                writer.NewVirtualVoidMethod<TT.TProperty>(GetWriteAccessorMethodName(MetaProperty))
                    .Implement((w, value) => _storageField.Assign(value.CastTo<TT.TValue>()));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                writer.ImplementBase<object>().NewVirtualWritableProperty<TT.TValue>(MetaProperty.Name).Implement(
                    p => p.Get(m => {
                        base.ImplementedStorageProperty = p.OwnerProperty.PropertyBuilder;
                        m.Return(_storageField);
                    }),
                    p => p.Set((m, value) =>
                        _storageField.Assign(value)
                    )
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            if ( MetaProperty.ClrType.IsEntityPartContract() )
            {
                using ( TT.CreateScope<TT.TValue>(_storageType) )
                {
                    _storageField.Assign(writer.New<TT.TValue>(components));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var m = writer;

            Static.Void(RuntimeTypeModelHelpers.DeepListNestedObject, _storageField, nestedObjects);

            //m.If(_storageField.IsNotNull()).Then(() => {
            //    nestedObjects.Add(_storageField);

            //    if ( typeof(IHaveNestedObjects).IsAssignableFrom(_storageType) )
            //    {
            //        _storageField.CastTo<IHaveNestedObjects>().Void(x => x.DeepListNestedObjects, nestedObjects);
            //    }
            //});
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        #endregion
    }
}
