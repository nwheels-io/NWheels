using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IUnitOfWork : IDisposable
    {
        void CommitChanges();
        //void RollbackChanges();
        bool IsAutoCommitMode { get; }
        UnitOfWorkState UnitOfWorkState { get; }
    }
}
