using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIEntityObjectConvention : ImplementationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIEntityObjectConvention(ITypeMetadata metaType)
            : base(Will.ImplementBaseClass)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (
                context.TypeKey.PrimaryInterface.IsEntityContract() && 
                _metaType.PrimaryKey.Properties[0].DeclaringContract == _metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var keyPropertyInfo = _metaType.PrimaryKey.Properties[0].ContractPropertyInfo;
            var keyBackingField = writer.OwnerClass.GetPropertyBackingField(keyPropertyInfo);

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
                });

        }

        #endregion
    }
}
