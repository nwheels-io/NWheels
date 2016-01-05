using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;

namespace NWheels.Stacks.MongoDb
{
    public class UnsignedNumericOverflowConvention : IMemberMapConvention
    {
        public void Apply(BsonMemberMap memberMap)
        {
            var memberType = memberMap.MemberType;
            var type = Nullable.GetUnderlyingType(memberType) ?? memberType;

            if ( type == typeof(UInt32) )
            {
                memberMap.SetSerializationOptions(GetOverflowSerializationOptions(BsonType.Int32));
            }
            else if ( type == typeof(UInt64) )
            {
                memberMap.SetSerializationOptions(GetOverflowSerializationOptions(BsonType.Int64));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name
        {
            get { return "Overflow for UInt32 and UInt64"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static RepresentationSerializationOptions GetOverflowSerializationOptions(BsonType bsonType)
        {
            return new RepresentationSerializationOptions(bsonType) {
                AllowOverflow = true
            };
        }
    }
}
