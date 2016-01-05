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

namespace NWheels.Stacks.MongoDb.Factories
{
    public class BsonStringEnumConvention : DecorationConvention
    {
        public BsonStringEnumConvention()
            : base(Will.DecorateProperties)
        {
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
        {
            if ( member.PropertyType.IsValueType && 
                 member.PropertyType.IsEnum &&
                 MongoEntityObjectFactory.IsPersistableObjectPropertyPersistedToDb(member.PropertyBuilder) )
            {
                decorate().Attribute<BsonRepresentationAttribute>(args => args.Arg(BsonType.String));
            }
        }

        #endregion
    }
}
