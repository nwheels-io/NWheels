using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects.Core.StorageTypes
{
    /// <summary>
    /// Indicates value state for entity property of a dual contract/storage data type.
    /// </summary>
    [Flags]
    public enum DualValueStates
    {
        /// <summary>
        /// Neither contract nor storage value is present
        /// </summary>
        None = 0,
        /// <summary>
        /// Contract value is present
        /// </summary>
        Contract = 0x01,
        /// <summary>
        /// Storage value is present
        /// </summary>
        Storage = 0x02
    }
}
