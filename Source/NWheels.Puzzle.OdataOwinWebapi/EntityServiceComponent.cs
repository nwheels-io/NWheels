using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;
using NWheels.Entities;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.UI;
using NWheels.UI.Endpoints;
using Owin;
using Microsoft.OData.Edm;

namespace NWheels.Puzzle.OdataOwinWebapi
{
    internal class EntityServiceComponent : LifecycleEventListenerBase
    {
        private readonly IApplicationEntityRepository _repository;
        private readonly ILogger _logger;
        private readonly ILifetimeScope _container;
        private IDisposable _host = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityServiceComponent(IEntityRepositoryEndpoint endpoint, Auto<ILogger> logger, IComponentContext componentContext)
        {
            _repository = endpoint.Contract;
            _logger = logger.Instance;
            _container = (ILifetimeScope)componentContext;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Activate()
        {
            const string url = "http://localhost:9000/";

            try
            {
                _host = WebApp.Start(url, ConfigureWebService);
                _logger.EntityServiceStarted(_repository.GetType().Name, url);
            }
            catch ( Exception e )
            {
                _logger.EntityServiceFailedToStart(_repository.GetType().Name, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Deactivate()
        {
            try
            {
                _host.Dispose();
                _logger.EntityServiceStopped(_repository.GetType().Name);
            }
            catch ( Exception e )
            {
                _logger.EntityServiceFailedToStop(_repository.GetType().Name, e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConfigureWebService(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.MapODataServiceRoute(routeName: "EntityService", routePrefix: "entity", model: BuildEdmModel());

            config.DependencyResolver = new AutofacWebApiDependencyResolver(_container);

            app.UseAutofacMiddleware(_container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEdmModel BuildEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();

            foreach ( var entityType in _repository.GetEntityTypesInRepository() )
            {
                modelBuilder.AddEntitySet(entityType.Name, modelBuilder.AddEntityType(entityType));

                var structuralType = modelBuilder.StructuralTypes.First(t => t.ClrType == entityType);

                foreach ( var utcProperty in entityType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)) )
                {
                    structuralType.AddProperty(utcProperty);                    
                }

                foreach ( var nonUtcProperty in entityType.GetProperties().Where(p => p.PropertyType == typeof(DateTime)) )
                {
                    structuralType.RemoveProperty(nonUtcProperty);
                }
            }

            return modelBuilder.GetEdmModel();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogInfo]
            void EntityServiceStarted(string repositoryName, string Url);
            [LogError]
            void EntityServiceFailedToStart(string repositoryName, Exception e);
            [LogInfo]
            void EntityServiceStopped(string repositoryName);
            [LogError]
            void EntityServiceFailedToStop(string repositoryName, Exception e);
        }
    }
}
