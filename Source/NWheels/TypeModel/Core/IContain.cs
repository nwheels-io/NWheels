using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    public interface IContain<out T> where T : class
    {
        T GetContainedObject();
    }
}
