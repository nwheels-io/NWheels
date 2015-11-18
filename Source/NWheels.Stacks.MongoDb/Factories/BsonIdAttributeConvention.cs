using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.DataObjects;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class BsonIdAttributeConvention : DecorationConvention
    {
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BsonIdAttributeConvention(ITypeMetadata metaType)
            : base(Will.DecorateProperties)
        {
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_metaType.EntityIdProperty != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnProperty(PropertyMember member, Func<Hapil.Decorators.PropertyDecorationBuilder> decorate)
        {
            if ( member.GetterMethod.IsPublic && member.Name == _metaType.EntityIdProperty.Name )
            {
                decorate().Attribute<BsonIdAttribute>();
            }
        }

        #endregion
    }
}
