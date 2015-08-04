using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using MongoDB.Bson;
using NWheels.DataObjects.Core.Factories;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class ObjectIdGeneratorConvention : DecorationConvention
    {
        private readonly PropertyImplementationStrategyMap _propertyStrategyMap;
        private readonly IPropertyMetadata _keyProperty;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectIdGeneratorConvention(PropertyImplementationStrategyMap propertyStrategyMap, ITypeMetadata metaType)
            : base(Will.DecorateConstructors)
        {
            _propertyStrategyMap = propertyStrategyMap;
            _keyProperty = (
                metaType.PrimaryKey != null ? 
                metaType.PrimaryKey.Properties.FirstOrDefault(p => p.ClrType == typeof(ObjectId) && p.DefaultValueGeneratorType == null) : 
                null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_keyProperty != null && !_propertyStrategyMap.IsImplementedByBaseEntity(_keyProperty.ContractPropertyInfo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnConstructor(MethodMember member, Func<ConstructorDecorationBuilder> decorate)
        {
            if ( IsInitializationConstructor(member) )
            {
                var backingField = member.OwnerClass.GetPropertyBackingField(_keyProperty.ContractPropertyInfo);

                decorate().OnSuccess(m => {
                    backingField.AsOperand<ObjectId>().Assign(Static.Func(ObjectId.GenerateNewId));
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsInitializationConstructor(MethodMember method)
        {
            return (method.Signature.ArgumentCount > 0);
        }
    }
}
