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

namespace NWheels.Tools.TestBoard.Modules.StartPage
{
    [Export(typeof(StartPageViewModel))]
    public class StartPageViewModel : Document
    {
        private readonly ICommandService _commandService;
        private readonly ICommandRouter _commandRouter;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public StartPageViewModel(ICommandService commandService, ICommandRouter commandRouter)
        {
            _commandRouter = commandRouter;
            _commandService = commandService;
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

        public void BeginLoadApp(RecentApp app)
        {
            MessageBox.Show("Now loading app from: " + app.BootConfigFilePath);
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

        public List<RecentApp> RecentApps { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnInitialize()
        {
            LoadRecentApps();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetRecentAppsFilePath()
        {
            return PathUtility.HostBinPath("ntest.mru");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LoadRecentApps()
        {
            var filePath = GetRecentAppsFilePath();

            if ( File.Exists(filePath) )
            {
                var fileStuctrue = JsonConvert.DeserializeObject<RecentAppsFileStructure>(File.ReadAllText(filePath));
                this.RecentApps = fileStuctrue.RecentApps;
            }
            else
            {
                this.RecentApps = new List<RecentApp>() {
                    new RecentApp() {
                        BootConfigFilePath = @"C:\PathOne\AppOne\boot.config",
                        BootConfigFileExists = true,
                        BootConfigIsValid = true,
                        BootConfig = new BootConfiguration() {
                            ApplicationName = "Test Application One",
                            NodeName = "AppServer",
                            EnvironmentName = "Local",
                            EnvironmentType = "DEV",
                            FrameworkModules = new List<BootConfiguration.ModuleConfig>() {
                                new BootConfiguration.ModuleConfig() { Name = "Framework.Module1" },
                                new BootConfiguration.ModuleConfig() { Name = "Framework.Module2" },
                            },
                            ApplicationModules = new List<BootConfiguration.ModuleConfig>() {
                                new BootConfiguration.ModuleConfig() { Name = "Application.Module1" },
                                new BootConfiguration.ModuleConfig() { Name = "Application.Module2" },
                            }
                        }
                    },
                    new RecentApp() {
                        BootConfigFilePath = @"C:\PathTwo\AppTwo\boot.config",
                        BootConfigFileExists = true,
                        BootConfigIsValid = true,
                        BootConfig = new BootConfiguration() {
                            ApplicationName = "Test Application Two",
                            NodeName = "ClientConnector",
                            EnvironmentName = "QA1",
                            EnvironmentType = "QA",
                            FrameworkModules = new List<BootConfiguration.ModuleConfig>() {
                                new BootConfiguration.ModuleConfig() { Name = "Framework.Module1" },
                                new BootConfiguration.ModuleConfig() { Name = "Framework.Module2" },
                            },
                            ApplicationModules = new List<BootConfiguration.ModuleConfig>() {
                                new BootConfiguration.ModuleConfig() { Name = "Application.Module1" },
                                new BootConfiguration.ModuleConfig() { Name = "Application.Module2" },
                            }
                        }
                    },
                };
            }

            ReloadBootConfigs();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SaveRecentApps()
        {
            var filePath = GetRecentAppsFilePath();

            if ( RecentApps != null )
            {
                var fileStuctrue = new RecentAppsFileStructure {
                    RecentApps = this.RecentApps
                };

                File.WriteAllText(filePath, JsonConvert.SerializeObject(fileStuctrue));
            }
            else if  ( File.Exists(filePath) )
            {
                File.Delete(filePath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReloadBootConfigs()
        {
            foreach ( var app in this.RecentApps )
            {
                app.BootConfigFileExists = File.Exists(app.BootConfigFilePath);

                if ( app.BootConfigFileExists )
                {
                    app.BootConfig = BootConfiguration.LoadFromFile(app.BootConfigFilePath);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RecentApp
        {
            public string BootConfigFilePath { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [JsonIgnore]
            public bool BootConfigFileExists { get; set; }
            [JsonIgnore]
            public bool BootConfigIsValid { get; set; }
            [JsonIgnore]
            public BootConfiguration BootConfig { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ApplicationText
            {
                get
                {
                    return (
                        BootConfig != null ? 
                        string.Format("{0} / {1}", BootConfig.ApplicationName, BootConfig.NodeName) : 
                        "N/A");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string EnvironmentText
            {
                get
                {
                    return (
                        BootConfig != null ?
                        string.Format("{0} (type {1})", BootConfig.EnvironmentName, BootConfig.EnvironmentType) :
                        "N/A");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ModulesText
            {
                get
                {
                    return (
                        BootConfig != null ?
                        string.Join(", ", BootConfig.FrameworkModules.Concat(BootConfig.ApplicationModules).Select(m => m.Name)) :
                        "N/A");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ErrorText
            {
                get
                {
                    return (
                        !BootConfigFileExists ? "Boot.config not found" : 
                        !BootConfigIsValid ? "Boot.config is not valid" : 
                        "");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasErrors
            {
                get
                {
                    return (!BootConfigFileExists || !BootConfigIsValid);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RecentAppsFileStructure
        {
            public List<RecentApp> RecentApps { get; set; }
        }
    }
}
