using NWheels.DB.Model;

namespace NWheels.DB.Adapters.MongoDB
{
    public static class MongoDBTechnologyAdapter
    {
        public static MongoDBDatabase AsMongoDBDatabase(this DatabaseModel db)
        {
            return new MongoDBDatabase();
        }

        public static DotNetOdmToMongoDB AsDotNetOdmToMongoDB(this DatabaseModel db)
        {
            return new DotNetOdmToMongoDB();
        }
    }
}
