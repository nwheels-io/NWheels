using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Core
{
    public interface IDataRepositoryCallback
    {
        void Invoke<TEntityContract, TEntityImpl>(IEntityRepository<TEntityContract> repo);
    }
}
