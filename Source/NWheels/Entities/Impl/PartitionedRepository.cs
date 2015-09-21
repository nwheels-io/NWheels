using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;
using NWheels.Logging;

namespace NWheels.Entities.Impl
{
    public class PartitionedRepository<TEntity, TPartition> : IPartitionedRepository<TEntity, TPartition>
    {
        private readonly Func<TPartition, IEntityRepository<TEntity>> _repositoryFactory;
        private readonly IDomainContextLogger _logger;
        private readonly ConcurrentDictionary<TPartition, IEntityRepository<TEntity>> _repositoryByPartitionValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PartitionedRepository(Func<TPartition, IEntityRepository<TEntity>> repositoryFactory, IDomainContextLogger logger)
        {
            _logger = logger;
            _repositoryFactory = repositoryFactory;
            _repositoryByPartitionValue = new ConcurrentDictionary<TPartition, IEntityRepository<TEntity>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IPartitionedRepository<TEntity,TPartition>

        public IEntityRepository<TEntity> this[TPartition partitionValue]
        {
            get
            {
                return _repositoryByPartitionValue.GetOrAdd(
                    partitionValue,
                    key => {
                        _logger.CreatRepositoryPartition(entity: typeof(TEntity), partitionValue: partitionValue);
                        return _repositoryFactory(key);
                    });
            }
        }

        #endregion
    }
}
