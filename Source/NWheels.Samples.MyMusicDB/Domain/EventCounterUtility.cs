using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Stacks.MongoDb;

namespace NWheels.Samples.MyMusicDB.Domain
{
    public static class EventCounterUtility
    {
        public const string ApiRequestCounterName = "ApiRequests";
        public const string UniqueUserCounterName = "UniqueUsers";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void IncrementEventCounters(this IMusicDBContext context, long deltaApiRequests, long deltaUniqueUsers)
        {
            var countersCollection = context.EventCounters.GetMongoCollection();
            var bulkWrite = countersCollection.InitializeUnorderedBulkOperation();

            DefineUpsert(bulkWrite, ApiRequestCounterName, deltaApiRequests);
            DefineUpsert(bulkWrite, UniqueUserCounterName, deltaUniqueUsers);

            bulkWrite.Execute(WriteConcern.Acknowledged);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void QueryEventCounters(this IMusicDBContext context, out long apiRequests, out long uniqueUsers)
        {
            var allCounters = context.EventCounters.AsQueryable().ToList();
            
            var apiRequestCounter = allCounters.FirstOrDefault(c => c.Id == ApiRequestCounterName);
            var uniqueUserCounter = allCounters.FirstOrDefault(c => c.Id == UniqueUserCounterName);

            apiRequests = (apiRequestCounter != null ? apiRequestCounter.Value : 0);
            uniqueUsers = (uniqueUserCounter != null ? uniqueUserCounter.Value : 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void DefineUpsert(BulkWriteOperation bulkWrite, string counterName, long delta)
        {
            var update = new UpdateBuilder();
            update.Inc("Value", delta);
            bulkWrite.Find(Query.EQ("_id", new BsonString(counterName))).Upsert().Update(update);
        }
    }
}