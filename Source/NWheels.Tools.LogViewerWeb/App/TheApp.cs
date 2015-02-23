using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NWheels.Logging;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
using System.Configuration;

namespace NWheels.Tools.LogViewerWeb.App
{
    public class TheApp : NancyModule
    {
        private readonly List<string> _configuredFolderPaths = new List<string>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TheApp()
        {
            PopulateConfiguredFolderPaths();

            base.Get["/app.js"] = _ => Response.AsText(LoadResource("app.js"), contentType: "application/javascript");
            base.Get["/style.css"] = _ => Response.AsText(LoadResource("style.css"), contentType: "text/css");
            base.Get["/threadlog/{logId:guid}"] = _ => Response.AsText(LoadResource("index.html"), contentType: "text/html");
            base.Get["/threadlog/{logId:guid}/json"] = _ => ServeThreadLog(_.logId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PopulateConfiguredFolderPaths()
        {
            for ( int i = 1 ; i < 100 ; i++ )
            {
                var path = ConfigurationManager.AppSettings["path." + i];

                if ( !string.IsNullOrWhiteSpace(path) )
                {
                    _configuredFolderPaths.Add(path);
                }
                else
                {
                    break;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response ServeThreadLog(Guid logId)
        {
            Console.WriteLine("Serving thread log ID {0}", logId);

            foreach ( var folderPath in _configuredFolderPaths )
            {
                var filePath = Path.Combine(folderPath, logId.ToString("N") + ".threadlog");

                Console.WriteLine("> trying: {0}", filePath);

                if ( File.Exists(filePath) )
                {
                    Console.WriteLine("> FOUND - serving.");

                    var serializer = new DataContractSerializer(typeof(ThreadLogSnapshot));

                    using ( var file = File.OpenRead(filePath) )
                    {
                        var snapshot = (ThreadLogSnapshot)serializer.ReadObject(file);
                        return Response.AsJson(snapshot);
                    }
                }
            }

            Console.WriteLine("> NOT FOUND!");
            return HttpStatusCode.NotFound;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string LoadResource(string fileName)
        {
            var filePath = GetResourceFilePath(fileName);

            Console.WriteLine("Loading resource '{0}' from path: {1}", fileName, filePath);

            return File.ReadAllText(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetResourceFilePath(string fileName)
        {
            var privateBinPath = Path.GetDirectoryName(typeof(TheApp).Assembly.Location); 
            return Path.Combine(Path.Combine(privateBinPath, "App"), fileName);
        }
    }
}
