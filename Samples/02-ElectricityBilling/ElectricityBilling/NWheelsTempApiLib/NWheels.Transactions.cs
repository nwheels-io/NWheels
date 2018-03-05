using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    namespace Transactions
    {
        public interface ITransactionFactory
        {
            IUnitOfWork NewUnitOfWork();
        }

        public interface IUnitOfWork : IDisposable
        {
            Task CommitAsync();
            Task DiscardAsync();
        }
    }

}
