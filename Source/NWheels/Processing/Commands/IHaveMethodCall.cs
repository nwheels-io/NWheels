using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Commands
{
    public interface IHaveMethodCall
    {
        IMethodCallObject Call { get; }
    }
}
