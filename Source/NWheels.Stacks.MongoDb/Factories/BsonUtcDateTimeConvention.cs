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
    public class BsonUtcDateTimeConvention : DecorationConvention
    {
        public BsonUtcDateTimeConvention()
            : base(Will.DecorateProperties)
        {
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnProperty(PropertyMember member, Func<PropertyDecorationBuilder> decorate)
        {
            if ( member.PropertyType == typeof(DateTime) &&
                 MongoEntityObjectFactory.IsPersistableObjectPropertyPersistedToDb(member.PropertyBuilder) )
            {
                decorate()
                    .Attribute<BsonDateTimeOptionsAttribute>(args => args.Named(x => x.Kind, DateTimeKind.Utc))
                    .Getter().OnReturnValue(
                        (w, retVal) => {
                            var retValAsDateTime = w.Local<DateTime>();
                            retValAsDateTime.Assign(retVal.CastTo<DateTime>());
                            retVal.Assign(
                                w.New<DateTime>(
                                    retValAsDateTime.Prop(x => x.Year),
                                    retValAsDateTime.Prop(x => x.Month),
                                    retValAsDateTime.Prop(x => x.Day),
                                    retValAsDateTime.Prop(x => x.Hour),
                                    retValAsDateTime.Prop(x => x.Minute),
                                    retValAsDateTime.Prop(x => x.Second),
                                    w.Const(DateTimeKind.Utc)
                                )
                                .CastTo<TypeTemplate.TReturn>()
                            );
                        });
            }
        }

        #endregion
    }
}
