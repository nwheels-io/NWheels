using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using NWheels.Entities;

namespace NWheels.Stacks.MongoDb
{
    public static class MongoEntityRepositoryExtensions
    {
        public static MongoCollection GetMongoCollection<T>(this IEntityRepository<T> repository)
        {
            return ((IMongoEntityRepository)repository).GetMongoCollection();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MongoCollection GetMongoCollection<TEntity, TPartition>(this IPartitionedRepository<TEntity, TPartition> partitioned, TPartition partition)
        {
            return ((IMongoEntityRepository)partitioned[partition]).GetMongoCollection();
        }
    }
}
