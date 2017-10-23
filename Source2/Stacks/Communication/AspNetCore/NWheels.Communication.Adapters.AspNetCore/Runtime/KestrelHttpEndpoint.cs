using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using NWheels.Communication.Api;
using NWheels.Communication.Api.Http;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;

namespace NWheels.Communication.Adapters.AspNetCore.Runtime
{
    public class KestrelHttpEndpoint : LifecycleComponentBase, IHttpEndpoint
    {
        private readonly IComponentContainer _components;
        private readonly IHttpEndpointConfigElement _configuration;
        private readonly string _name;
        //private Func<HttpContext, Task> _handler;
        private ImmutableArray<string> _listenUrls;
        private volatile IWebHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public KestrelHttpEndpoint(IComponentContainer components,  IHttpEndpointConfigElement configuration)
        {
            _components = components;
            _configuration = configuration;
            _name = configuration.Name;
            //TODO: _handler = port.OnRequest;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void AddMiddleware(ICommunicationMiddleware<HttpContext> middleware)
        {
            if (_host != null)
            {
                throw new InvalidOperationException("Cannot subscribe handlers while active.");
            }

            //TODO: _handler += handler;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name => _name;

        public string Protocol => _s_protocolName;

        public IEnumerable<string> ListenUrls => _listenUrls;

        public IEnumerable<ICommunicationMiddleware> Pipeline => throw new NotImplementedException();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            base.Activate();

            var builder = new WebHostBuilder();

            BuildListenUrls();

            builder.UseKestrel(ConfigureKestrelServer);
            //builder.UseUrls(_listenUrls.ToArray());
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
                .Add($"http://0.0.0.0:{_configuration.Port}");

            if (_configuration.Https != null)
            {
                _listenUrls = _listenUrls.Add($"https://0.0.0.0:{_configuration.Https.Port}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureKestrelServer(KestrelServerOptions options)
        {
            options.Listen(IPAddress.Any, _configuration.Port, listenOptions => {
                listenOptions.NoDelay = true;
            });

            if (_configuration.Https != null)
            {
                _listenUrls = _listenUrls.Add($"https://0.0.0.0:{_configuration.Https.Port}");

                options.Listen(IPAddress.Any, _configuration.Https.Port, listenOptions =>
                {
                    var fullCertFilePath = PathUtility.ExpandPathFromBinary(_configuration.Https.CertFilePath);
                    
                    listenOptions.NoDelay = true;
                    listenOptions.UseHttps(fullCertFilePath, _configuration.Https.CertFilePassword);
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureWebHost(IApplicationBuilder app)
        {
            if (_configuration.StaticFolders != null)
            {
                ConfigureStaticFolders(app);
            }

            foreach (var middlewareType in _configuration.MiddlewarePipeline)
            {
                var middlewareInstance = (ICommunicationMiddleware<HttpContext>)_components.Resolve(middlewareType);
                app.Use(middlewareInstance.OnMessage);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ConfigureStaticFolders(IApplicationBuilder app)
        {
            foreach (var folder in _configuration.StaticFolders)
            {
                var safeRequestPath = (
                    folder.RequestBasePath == "/"
                    ? string.Empty
                    : folder.RequestBasePath.DefaultIfNullOrEmpty(string.Empty));

                var folderOptions = new FileServerOptions {
                    FileProvider = new PhysicalFileProvider(GetStaticWebContentFolderPath(folder)),
                    RequestPath = new PathString(safeRequestPath),
                    EnableDefaultFiles = (folder.DefaultFiles?.Count > 0),
                    EnableDirectoryBrowsing = folder.EnableDirectoryBrowsing
                };

                folderOptions.StaticFileOptions.DefaultContentType = folder.DefaultContentType;

                if (folder.EnableDirectoryBrowsing)
                {
                    folderOptions.DirectoryBrowserOptions.FileProvider = new PhysicalFileProvider(GetStaticWebContentFolderPath(folder));
                    folderOptions.DirectoryBrowserOptions.RequestPath = folder.RequestBasePath.DefaultIfNullOrEmpty("/");
                }

                if (folderOptions.EnableDefaultFiles)
                {
                    folderOptions.DefaultFilesOptions.FileProvider = new PhysicalFileProvider(GetStaticWebContentFolderPath(folder));
                    folderOptions.DefaultFilesOptions.RequestPath = new PathString(safeRequestPath);
                    folderOptions.DefaultFilesOptions.DefaultFileNames = folder.DefaultFiles.ToArray();
                }

                app.UseFileServer(folderOptions);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetStaticWebContentFolderPath(IHttpStaticFolderConfig folder)
        {
            var path = PathUtility.ExpandPathFromBinary(folder.LocalRootPath);

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

            //TODO: return _handler(context);
            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string _s_protocolName = "http";
    }
}
