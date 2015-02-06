using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{

    public interface IDataRepositoryEndpoint
    {
        IApplicationDataRepository Contract { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DataRepositoryEndpoint<TRepository> : IDataRepositoryEndpoint 
        where TRepository : IApplicationDataRepository
    {
        private readonly TRepository _repository;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DataRepositoryEndpoint(TRepository repository)
        {
            _repository = repository;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IApplicationDataRepository Contract
        {
            get { return _repository; }
        }
    }
}
