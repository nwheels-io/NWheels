using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Testing.Logging.Core
{
    public interface ILogexNodeMatcher
    {
        bool Match(LogNode node);
        bool MatchEndOfInput();
        string Describe();
    }
}
