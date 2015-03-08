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
using Nancy.ModelBinding;

namespace NWheels.Tools.LogViewerWeb.App
{
    public class TheApp : NancyModule
    {
        public TheApp()
        {
            base.Get["/app.js"] = _ => Response.AsText(LoadResource("app.js"), contentType: "application/javascript");
            base.Get["/style.css"] = _ => Response.AsText(LoadResource("style.css"), contentType: "text/css");
            base.Get["/"] = _ => Response.AsText(LoadResource("index.html"), contentType: "text/html");
            base.Get["/threadlog/{logId:guid}"] = _ => Response.AsText(LoadResource("index.html"), contentType: "text/html");
            base.Get["/threadlog/{logId:guid}/json"] = _ => ServeThreadLog(_.logId);
            base.Post["/capture"] = _ => ServeCapturedLogs(this.Bind<FetchRequest>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response ServeCapturedLogs(FetchRequest request)
        {
            Console.WriteLine("Serving captured thread logs for Last Capture ID = {0}", request.LastCaptureId);

            long responseCaptureId = 0;
            var responseLogs = new List<ThreadNodeViewModel>();

            foreach ( var folder in Program.FolderWatchers )
            {
                var lastCaptureId = request.LastCaptureId;
                var logsFromFolder = folder.GetCapturedLogs(ref lastCaptureId);
                
                responseLogs.AddRange(logsFromFolder);

                if ( lastCaptureId > responseCaptureId )
                {
                    responseCaptureId = lastCaptureId;
                }

                Console.WriteLine("> CAPTURED {0} new logs in: {1}", logsFromFolder.Length, folder.FolderPath);
            }

            responseLogs.Sort((x, y) => x.TimestampValue.CompareTo(y.TimestampValue));

            return Response.AsJson(new FetchResponse {
                LastCaptureId = responseCaptureId,
                Logs = responseLogs.ToArray()
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Response ServeThreadLog(Guid logId)
        {
            Console.WriteLine("Serving thread log ID {0}", logId);

            foreach ( var folder in Program.FolderWatchers )
            {
                Console.WriteLine("> trying: {0}", folder.FolderPath);

                ThreadNodeViewModel log;

                if ( folder.TryGetLogById(logId, out log) )
                {
                    Console.WriteLine("> FOUND - serving.");
                    return Response.AsJson(log);
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
