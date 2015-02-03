using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.Hosting
{
    public interface IInitializableHostComponent
    {
        void Initializing();
        void Configured();
    }
}
