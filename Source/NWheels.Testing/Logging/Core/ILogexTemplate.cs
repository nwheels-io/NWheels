using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Testing.Logging.Impl;

namespace NWheels.Testing.Logging.Core
{
    internal interface ILogexTemplate
    {
        LogexSegment[] CreateSegments(ILogexNodeMatcher matcher);
    }
}
