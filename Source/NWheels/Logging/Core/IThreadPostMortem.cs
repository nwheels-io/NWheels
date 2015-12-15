using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging.Core
{
    public interface IThreadPostMortem
    {
        void Examine(IReadOnlyThreadLog threadLog);
    }
}
