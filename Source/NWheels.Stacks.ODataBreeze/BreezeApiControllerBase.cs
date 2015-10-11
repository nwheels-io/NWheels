#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Autofac;
using Breeze.ContextProvider;
using Breeze.WebApi2;
using Newtonsoft.Json.Linq;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Concurrency;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    [BreezeController]
    [EnableCors("*", "*", "*")]
    public abstract class BreezeApiControllerBase<TDataRepo> : ApiController
        where TDataRepo : class, IApplicationDataRepository
    {
        private readonly IFramework _framework;
        private readonly BreezeContextProvider<TDataRepo> _contextProvider;
        private readonly IBreezeEndpointLogger _logger;
        private readonly ThreadStaticAnchor<PerContextResourceConsumerScope<TDataRepo>> _domainContextAnchor;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BreezeApiControllerBase(IComponentContext components, IFramework framework, ITypeMetadataCache metadataCache)
        {
            _framework = framework;
            _logger = components.Resolve<IBreezeEndpointLogger>();
            _contextProvider = new BreezeContextProvider<TDataRepo>(components, framework, metadataCache, _logger);
            _domainContextAnchor = new ThreadStaticAnchor<PerContextResourceConsumerScope<TDataRepo>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("Metadata")]
        public string Metadata()
        {
            try
            {
                var metadataJsonString = _contextProvider.Metadata();
                return metadataJsonString;
            }
            finally
            {
                _domainContextAnchor.Clear();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [HttpPost]
        [Route("SaveChanges")]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            //TODO: remove this once we are sure the bug is solved
            PerContextResourceConsumerScope<TDataRepo> stale;
            if ( (stale = _domainContextAnchor.Current) != null )
            {
                _logger.StaleUnitOfWorkEncountered(stale.Resource.ToString(), ((DataRepositoryBase)(object)stale.Resource).InitializerThreadText);
            }

            using ( var activity = _logger.RestWriteInProgress() )
            {
                var dataRepoAnchor = new ThreadStaticAnchor<DataRepositoryBase>();

                try
                {
                    SaveResult result;

                    using ( var context = _framework.NewUnitOfWork<TDataRepo>() )
                    {
                        dataRepoAnchor.Current = (DataRepositoryBase)(object)context;
                        result = _contextProvider.SaveChanges(saveBundle);
                        context.CommitChanges();
                    }

                    _logger.RestWriteCompleted();
                    return result;
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    _logger.RestWriteFailed(e);
                    throw;
                }
                finally
                {
                    dataRepoAnchor.Clear();
                    _domainContextAnchor.Clear();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ApiController

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if ( disposing )
            {
                _contextProvider.Dispose();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public BreezeContextProvider<TDataRepo> ContextProvider
        {
            get { return _contextProvider; }
        }
    }
}
