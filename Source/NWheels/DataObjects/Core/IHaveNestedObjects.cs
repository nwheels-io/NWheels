using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects.Core
{
    public interface IHaveNestedObjects
    {
        void DeepListNestedObjects(HashSet<object> nestedObjects);
    }
}
