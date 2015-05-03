using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Testing.Logging.Core
{
    public interface ILogexMultiplier
    {
        bool IsSatisfied(int matchCount);
        bool IsAtMax(int matchCount);
        string Describe();
    }
}
