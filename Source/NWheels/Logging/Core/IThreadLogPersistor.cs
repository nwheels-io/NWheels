using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging.Core
{
    /// <summary>
    /// Defines a persistence provider for thread logs
    /// </summary>
    public interface IThreadLogPersistor
    {
        void Persist(IReadOnlyThreadLog threadLog);
    }
}
