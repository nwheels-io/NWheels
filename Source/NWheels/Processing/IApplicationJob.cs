using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface IApplicationJob
    {
        void Execute(IApplicationJobContext context);
        string JobId { get; }
        string Description { get; }
        bool IsReentrant { get; }
        bool NeedsPersistence { get; }
    }
}
