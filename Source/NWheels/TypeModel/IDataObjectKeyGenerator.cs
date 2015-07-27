using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IDataObjectKeyGenerator
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDataObjectKeyGenerator<out T> : IDataObjectKeyGenerator
    {
        T TakeNextKey();
    }
}
