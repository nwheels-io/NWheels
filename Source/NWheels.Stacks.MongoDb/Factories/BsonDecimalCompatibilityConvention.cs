using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.TypeModel.Core.StorageTypes;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class BsonDecimalCompatibilityConvention : DecorationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BsonDecimalCompatibilityConvention(ITypeMetadata metaType)
            : base(Will.DecorateProperties)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
        {
            IPropertyMetadata metaProperty;

            if (member.PropertyDeclaration == null && _metaType.TryGetPropertyByName(member.Name, out metaProperty))
            {
                if (metaProperty.RelationalMapping != null && metaProperty.RelationalMapping.StorageType is DecimalAsInt64StorageType)
                {
                    decorate().Attribute<BsonSerializerAttribute>(args => args.Arg(typeof(DecimalAsInt64StringCompatibleSerializer)));
                }
            }
        }

        #endregion
    }
}
