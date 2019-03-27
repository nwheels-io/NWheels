using System.Threading.Tasks;
using MongoDB.Driver;

namespace TodoList.BackendService.Repositories
{
    public static class MongoDBHelpers
    {
        public static IMongoDatabase Connect(MongoDBConfig config)
        {
            var credential = MongoCredential.CreateCredential(
                config.Credential.Database, 
                config.Credential.UserName, 
                config.Credential.Password);
            
            var client = new MongoClient(new MongoClientSettings() {
                Server = new MongoServerAddress(config.Host),
                Credential = credential
            });
            
            var database = client.GetDatabase(config.Database);
            return database;
        }

        public static async Task<int> TakeNextSequenceNumber(this IMongoDatabase db, string id)
        {
            var counter = await GetSequenceCountersCollection(db).FindOneAndUpdateAsync(
                Builders<SequenceCounter>.Filter.Eq(x => x.Id, id),
                Builders<SequenceCounter>.Update.Inc(x => x.NextValue, 1),
                new FindOneAndUpdateOptions<SequenceCounter> {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                }
            );

            return counter.NextValue;
        }
        
        private static IMongoCollection<SequenceCounter> GetSequenceCountersCollection(IMongoDatabase db)
        {
            return db.GetCollection<SequenceCounter>("sequence_counters");
        }

        public class SequenceCounter
        {
            public string Id;
            public int NextValue;
        }
    }
}