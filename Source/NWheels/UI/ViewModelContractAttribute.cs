using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;

namespace NWheels.UI
{
    public class ViewModelContractAttribute : DataObjectContractAttribute
    {
        #region Overrides of DataObjectContractAttribute

        public override void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache cache)
        {
            base.ApplyTo(type, cache);
            type.IsViewModel = true;
        }

        #endregion
    }
}
