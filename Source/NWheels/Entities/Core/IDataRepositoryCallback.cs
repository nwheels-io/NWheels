using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Core
{
    public interface IDataRepositoryCallback
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDataRepositoryCallback<TEntityContract> : IDataRepositoryCallback
    {
        void Invoke<TEntityImpl>(IEntityRepository<TEntityContract> repo) where TEntityImpl : TEntityContract;
    }
}
