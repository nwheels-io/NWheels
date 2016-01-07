using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;

namespace NWheels.DataObjects.Core
{
    /// <summary>
    /// Accompanies storage data type objects (implementors of IStorageDataType), 
    /// by adding in-place code generation capability for dynamically generated data objects,
    /// so that storage data type object is not required at run-time.
    /// </summary>
    public interface IStorageContractConversionWriter
    {
        void WriteContractToStorageConversion(
            IPropertyMetadata metaProperty, 
            MethodWriterBase method, 
            IOperand<TypeTemplate.TContract> contractValue, 
            MutableOperand<TypeTemplate.TValue> storageValue);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void WriteStorageToContractConversion(
            IPropertyMetadata metaProperty, 
            MethodWriterBase method, 
            MutableOperand<TypeTemplate.TContract> contractValue, 
            IOperand<TypeTemplate.TValue> storageValue);
    }
}
