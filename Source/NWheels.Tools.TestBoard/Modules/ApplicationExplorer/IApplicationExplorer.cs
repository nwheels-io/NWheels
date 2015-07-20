using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using NWheels.Tools.TestBoard.Services;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    public interface IApplicationExplorer : ITool
    {
        ApplicationState CurrentState { get; }
    }
}
