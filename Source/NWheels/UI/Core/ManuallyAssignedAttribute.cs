using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    /// <summary>
    /// Suppresses automatic instantiation of UIDL node referenced by current property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ManuallyAssignedAttribute : Attribute
    {
    }
}
