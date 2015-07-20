using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using NWheels.Hosting;
using NWheels.Utilities;
using System.IO;
using System.Windows;
using Gemini.Framework.Commands;
using Newtonsoft.Json;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Tools.TestBoard.Services;

namespace NWheels.Tools.TestBoard.Modules.StartPage
{
    [Export(typeof(StartPageViewModel))]
    public class StartPageViewModel : Document
    {
        private readonly ICommandService _commandService;
        private readonly ICommandRouter _commandRouter;
        private readonly IRecentAppListService _recentAppList;
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public StartPageViewModel(
            ICommandService commandService, 
            ICommandRouter commandRouter, 
            IRecentAppListService recentAppList,
            IApplicationControllerService controller)
        {
            _commandRouter = commandRouter;
            _commandService = commandService;
            _recentAppList = recentAppList;
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadNewApp()
        {
            var commandDefinition = _commandService.GetCommandDefinition(typeof(LoadNewApplicationCommandDefinition));
            var command = _commandService.GetCommand(commandDefinition);
            var commandHandler = _commandRouter.GetCommandHandler(_commandService.GetCommandDefinition(typeof(LoadNewApplicationCommandDefinition)));
            commandHandler.Run(command);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LoadRecentApp(IRecentApp app)
        {
            if ( _controller.CanLoad() )
            {
                _controller.LoadAsync(app.BootConfigFilePath);
            }
            else
            {
                MessageBox.Show("Cannot load application at this moment.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool ShouldReopenOnStart
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string DisplayName
        {
            get { return "Start Page"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IRecentApp> RecentApps
        {
            get
            {
                return _recentAppList.GetRecentApps();
            }
        }
    }
}
