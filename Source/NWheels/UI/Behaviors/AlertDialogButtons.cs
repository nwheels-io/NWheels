using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    [Flags]
    public enum AlertDialogButtons
    {
        Ok = 0x01,
        Yes = 0x02,
        No = 0x04,
        Cancel = 0x08,
        YesNo = Yes | No,
        YesNoCancel = Yes | No | Cancel,
        OkCancel = Ok | Cancel
    }
}
