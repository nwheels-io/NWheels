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

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public static void CreateSearchIndex(MongoDatabase database, string collectionName, string indexName, string[] propertyNames)
        //{
        //    var options = new IndexOptionsBuilder();
        //    options.SetBackground(true);

        //    if (!string.IsNullOrEmpty(indexName))
        //    {
        //        options.SetName(indexName);
        //    }

        //    database.GetCollection(collectionName).CreateIndex(
        //        new IndexKeysBuilder().Ascending(propertyNames),
        //        options);
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public static void CreateUniqueIndex(MongoDatabase database, string collectionName, string indexName, string[] propertyNames)
        //{
        //    var options = new IndexOptionsBuilder();
        //    options.SetBackground(true);
        //    options.SetUnique(true);

        //    if (!string.IsNullOrEmpty(indexName))
        //    {
        //        options.SetName(indexName);
        //    }

        //    database.GetCollection(collectionName).CreateIndex(
        //        new IndexKeysBuilder().Ascending(propertyNames), 
        //        options);
        //}
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void CreateIndexOnCollection(MongoDatabase database, string collectionName, string indexName, bool unique, string[] propertyNames)
        {
            var options = new IndexOptionsBuilder();
            options.SetBackground(true);

            if (unique)
            {
                options.SetUnique(true);
            }

            if (!string.IsNullOrEmpty(indexName))
            {
                options.SetName(indexName);
            }

            database.GetCollection(collectionName).CreateIndex(
                new IndexKeysBuilder().Ascending(propertyNames),
                options);
        }
    }
}
