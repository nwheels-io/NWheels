using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{

    public interface IEntityRepositoryEndpoint
    {
        IApplicationEntityRepository Contract { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityRepositoryEndpoint<TRepository> : IEntityRepositoryEndpoint 
        where TRepository : IApplicationEntityRepository
    {
        private readonly TRepository _repository;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityRepositoryEndpoint(TRepository repository)
        {
            _repository = repository;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationEntityRepository Contract
        {
            get { return _repository; }
        }
    }
}
