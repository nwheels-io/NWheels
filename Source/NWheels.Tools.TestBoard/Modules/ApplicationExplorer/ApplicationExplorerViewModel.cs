using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using NWheels.Tools.TestBoard.Services;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    [Export(typeof(IApplicationExplorer))]
    public class ApplicationExplorerViewModel : Tool, IApplicationExplorer
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationExplorerViewModel(IShell shell, IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationState CurrentState
        {
            get
            {
                return _controller.CurrentState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return "Application Explorer"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool ShouldReopenOnStart
        {
            get { return true; }
        }
    }
}
