//using System;
//
//namespace NWheels.DB.Model
//{
//    public static class DatabaseMapper
//    {
//        public static DatabaseMapper<TDB> Of<TDB>() 
//            where TDB : DatabaseModel
//            => default;
//    }
//
//    public sealed class DatabaseMapper<TDB>
//        where TDB : DatabaseModel
//    {
//        private DatabaseMapper()
//        {
//        }
//
//        public ObjectMapper<TRecord> Collection<TRecord>(Func<TDB, DBCollection<TRecord>> selector) 
//            => default;
//    }
//
//    public sealed class ObjectMapper<T>
//    {
//        private ObjectMapper()
//        {
//        }
//    }
//}
