using System;
using Autofac;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIEntityObjectConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;
        private readonly PropertyImplementationStrategyMap _propertStrategyrMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIEntityObjectConvention(ITypeMetadata metaType, PropertyImplementationStrategyMap propertStrategyrMap)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
            _propertStrategyrMap = propertStrategyrMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (
                context.TypeKey.PrimaryInterface.IsEntityContract() && 
                _metaType.PrimaryKey != null &&
                _metaType.PrimaryKey.Properties[0].DeclaringContract == _metaType &&
                !_propertStrategyrMap.IsImplementedByBaseEntity(_metaType.PrimaryKey.Properties[0].ContractPropertyInfo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var keyPropertyInfo = _metaType.PrimaryKey.Properties[0].ContractPropertyInfo;
            var keyBackingField = writer.OwnerClass.GetPropertyBackingField(keyPropertyInfo);
            var domainObjectField = writer.Field<IDomainObject>("$domainObject");

            writer.ImplementInterfaceExplicitly<IEntityObject>()
                .Method<IEntityId>(intf => intf.GetId).Implement(f => {
                    using ( TT.CreateScope<TT.TContract, TT.TKey>(_metaType.ContractType, keyPropertyInfo.PropertyType) )
                    {
                        f.Return(f.New<EntityId<TT.TContract, TT.TKey>>(keyBackingField.AsOperand<TT.TKey>()));
                    }
                })
                .Method<object>(intf => intf.SetId).Implement((m, value) => {
                    using ( TT.CreateScope<TT.TContract, TT.TKey>(_metaType.ContractType, keyPropertyInfo.PropertyType) )
                    {
                        keyBackingField.AsOperand<TT.TKey>().Assign(value.CastTo<TT.TKey>());
                    }
                })
                .Method<IDomainObject>(x => x.SetContainerObject).Implement((m, domainObject) => {
                    domainObjectField.Assign(domainObject);
                })
                .Method<IDomainObject>(x => x.GetContainerObject).Implement(m => {
                    m.Return(domainObjectField);
                });
        }

        #endregion
    }
}
