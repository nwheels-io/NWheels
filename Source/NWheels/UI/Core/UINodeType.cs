using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public enum UINodeType
    {
        NotSet,
        Application,
        Screen,
        ScreenPart,
        Widget,
        Presenter,
        Command,
        Notification,
        UserAlert,
        Behavior,
        DataBinding
    }
}
