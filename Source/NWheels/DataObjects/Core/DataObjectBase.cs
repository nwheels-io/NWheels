using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects.Core
{
    public abstract class DataObjectBase : IDataObject
    {
        ITypeMetadata IDataObject.GetTypeMetadata()
        {
            throw new NotImplementedException();
        }

        IDataObjectKey IDataObject.GetKey()
        {
            throw new NotImplementedException();
        }
    }
}
