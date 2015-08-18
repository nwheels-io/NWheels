using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace NWheels.Stacks.MongoDb
{
    public static class RuntimeHelpers
    {
        // TODO: provide Static.GenericVoid in Hapil, in order to provide ability to call a void generic static method.
        public static object RegisterBsonClassMapIfNotYet<TPersistable>()
        {
            if ( !BsonClassMap.IsClassMapRegistered(typeof(TPersistable)) )
            {
                BsonClassMap.RegisterClassMap<TPersistable>();
            }

            return null;
        }
    }
}
