using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void CreateSearchIndex(MongoDatabase database, string collectionName, string propertyName)
        {
            database.GetCollection(collectionName).CreateIndex(
                new IndexKeysBuilder().Ascending(propertyName),
                new IndexOptionsBuilder());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void CreateUniqueIndex(MongoDatabase database, string collectionName, string propertyName)
        {
            database.GetCollection(collectionName).CreateIndex(
                new IndexKeysBuilder().Ascending(propertyName), 
                new IndexOptionsBuilder().SetUnique(true));
        }
    }
}
