using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Newtonsoft.Json;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Utilities;

namespace NWheels.Tools.TestBoard.Services
{
    public interface IRecentAppListService
    {
        IEnumerable<IRecentApp> GetRecentApps();
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IRecentApp
    {
        string BootConfigFilePath { get; }
        bool BootConfigFileExists { get; }
        bool BootConfigIsValid { get; }
        BootConfiguration BootConfig { get; }
        string ApplicationText { get; }
        string EnvironmentText { get; }
        string ModulesText { get; }
        string ErrorText { get; }
        bool HasErrors { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [Export(typeof(IRecentAppListService))]
    [Export(typeof(IHandle<AppOpenedMessage>))]
    public class RecentAppListService : IHandle<AppOpenedMessage>, IRecentAppListService
    {
        private readonly IEventAggregator _eventAggregator;
        private List<RecentApp> _recentApps;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public RecentAppListService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            LoadRecentApps();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<IRecentApp> IRecentAppListService.GetRecentApps()
        {
            return _recentApps;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppOpenedMessage>.Handle(AppOpenedMessage message)
        {
            if ( !_recentApps.Any(app => app.BootConfigFilePath.EqualsIgnoreCase(message.App.BootConfig.LoadedFromFile)) )
            {
                _recentApps.Add(new RecentApp() {
                    BootConfig = message.App.BootConfig,
                    BootConfigFilePath = message.App.BootConfig.LoadedFromFile,
                    BootConfigIsValid = true,
                    BootConfigFileExists = true
                });

                SaveRecentApps();
            }
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
                this._recentApps = fileStuctrue.RecentApps;
            }
            else
            {
                this._recentApps = new List<RecentApp>();
            }

            ReloadBootConfigs();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SaveRecentApps()
        {
            var filePath = GetRecentAppsFilePath();

            if ( _recentApps != null )
            {
                var fileStuctrue = new RecentAppsFileStructure {
                    RecentApps = this._recentApps
                };

                File.WriteAllText(filePath, JsonConvert.SerializeObject(fileStuctrue));
            }
            else if ( File.Exists(filePath) )
            {
                File.Delete(filePath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReloadBootConfigs()
        {
            foreach ( var app in this._recentApps )
            {
                app.BootConfigFileExists = File.Exists(app.BootConfigFilePath);

                if ( app.BootConfigFileExists )
                {
                    app.BootConfig = BootConfiguration.LoadFromFile(app.BootConfigFilePath);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RecentApp : IRecentApp
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
