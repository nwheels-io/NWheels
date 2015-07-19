using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    public interface IApplicationExplorer : ITool
    {
        ApplicationController Controller { get; }
        ApplicationState CurrentState { get; }
    }
}
