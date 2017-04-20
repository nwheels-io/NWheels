using NWheels.Microservices;
using NWheels.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace NWheels.Platform.Messaging.Adapters.AspNetKestrel
{
    public class KestrelHttpEndpoint : LifecycleListenerComponentBase, IEndpoint<HttpContext>
    {
        private readonly string _name;
        private readonly IHttpEndpointConfiguration _configuration;
        private Func<HttpContext, Task> _handler;
        private ImmutableArray<string> _listenUrls;
        private volatile IWebHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public KestrelHttpEndpoint(string name, HttpEndpointInjectorPort port)
        {
            _name = name;
            _configuration = port.Configuration;
            _handler = port.Handler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Subscribe(Func<HttpContext, Task> handler)
        {
            if (_host != null)
            {
                throw new InvalidOperationException("Cannot subscribe handlers while active.");
            }

            _handler += handler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name => _name;
        public IEnumerable<string> ListenUrls => _listenUrls;
        public string ProtocolFamily => _s_protocolFamily;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            base.Activate();

            var builder = new WebHostBuilder();

            BuildListenUrls();

            builder.UseKestrel(ConfigureKestrelServer);
            builder.UseUrls(_listenUrls.ToArray());
            builder.Configure(ConfigureWebHost);

            _host = builder.Build();
            _host.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void MayDeactivate()
        {
            base.MayDeactivate();

            _host.Dispose();
            _host = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildListenUrls()
        {
            _listenUrls = ImmutableArray<string>
                .Empty
                .Add($"http://localhost:{_configuration.Port}");

            if (_configuration.Https != null)
            {
                _listenUrls = _listenUrls.Add($"https://localhost:{_configuration.Https.Port}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureKestrelServer(KestrelServerOptions options)
        {
            //TODO: many interesting options exposed in .NET Core 2.0:
            //https://github.com/aspnet/KestrelHttpServer/blob/dev/samples/SampleApp/Startup.cs

            if (_configuration.Https != null)
            {
                options.NoDelay = true;
                options.UseHttps(_configuration.Https.CertFilePath, _configuration.Https.CertFilePassword);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureWebHost(IApplicationBuilder app)
        {
            if (_configuration.StaticFolders != null)
            {
                ConfigureStaticFolders(app);
            }

            if (_handler != null)
            {
                app.Run(HandleNonStaticRequest);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureStaticFolders(IApplicationBuilder app)
        {
            foreach (var folder in _configuration.StaticFolders)
            {
                var options = new StaticFileOptions() {
                    FileProvider = new PhysicalFileProvider(GetStaticWebContentFolderPath(folder)),
                    RequestPath = folder.RequestBasePath.DefaultIfNullOrEmpty("/"),
                    DefaultContentType = folder.DefaultContentType
                };

                app.UseStaticFiles(options);

                if (folder.EnableDirectoryBrowsing)
                {
                    var browserOptions = new DirectoryBrowserOptions() {
                        FileProvider = new PhysicalFileProvider(GetStaticWebContentFolderPath(folder)),
                        RequestPath = folder.RequestBasePath.DefaultIfNullOrEmpty("/")
                    };

                    app.UseDirectoryBrowser(browserOptions);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetStaticWebContentFolderPath(IHttpStaticFolderConfig folder)
        {
            var path = folder.LocalRootPath.DefaultIfNullOrEmpty(Directory.GetCurrentDirectory());

            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return Path.Combine(Directory.GetCurrentDirectory(), path);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Task HandleNonStaticRequest(HttpContext context)
        {
            //TODO: add logging and error handling

            return _handler(context);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_protocolFamily = "http";
    }
}
