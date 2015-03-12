using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Nancy.Json;
using NWheels.Tools.LogViewerWeb.App;
using Owin;

namespace NWheels.Tools.LogViewerWeb
{
    class Program
    {
        private static readonly List<LogFolderWatcher> _s_folderWatchers = new List<LogFolderWatcher>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static List<LogFolderWatcher> FolderWatchers
        {
            get { return _s_folderWatchers; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static void Main(string[] args)
        {
            CreateFolderWatchers();

            var url = "http://+:8999";

            JsonSettings.MaxJsonLength = Int32.MaxValue;
            JsonSettings.MaxRecursions = Int32.MaxValue;

            using ( WebApp.Start<Startup>(url) )
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }

            _s_folderWatchers.ForEach(watcher => watcher.Dispose());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void CreateFolderWatchers()
        {
            for ( int i = 1; i < 100; i++ )
            {
                var path = ConfigurationManager.AppSettings["path." + i];

                if ( !string.IsNullOrWhiteSpace(path) )
                {
                    _s_folderWatchers.Add(new LogFolderWatcher(path, captureExpiration: TimeSpan.FromMinutes(3), maxStoredCaptures: 100));
                }
                else
                {
                    break;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseNancy();
            }
        }
    }
}
