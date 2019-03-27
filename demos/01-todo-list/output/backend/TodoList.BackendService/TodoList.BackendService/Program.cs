using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#region NWheels.DevOps.Adapter.DotNetCore

namespace TodoList.BackendService
{
    public class Program
    {
        public static void Main(string[] args)
        {    
            #region NWheels.RestApi.Adapter.AspNetCore
            CreateWebHostBuilder(args).Build().Run();
            #endregion
        }

        #region NWheels.RestApi.Adapter.AspNetCore
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseKestrel(options => options.ListenLocalhost(port: 3001))
            .UseStartup<Startup>();
        #endregion
    }
}

#endregion
