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
    }
}