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
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IBreezeEndpointLogger _logger;
        private readonly ThreadStaticAnchor<PerContextResourceConsumerScope<TDataRepo>> _domainContextAnchor;
        private readonly ThreadStaticAnchor<DataRepositoryBase> _dataRepoAnchor;
        private readonly BreezeContextProvider<TDataRepo> _contextProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BreezeApiControllerBase(IComponentContext components, IFramework framework, ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _components = components;
            _framework = framework;
            _logger = components.Resolve<IBreezeEndpointLogger>();
            _contextProvider = new BreezeContextProvider<TDataRepo>(_components, _framework, _metadataCache, _logger);
            _domainContextAnchor = new ThreadStaticAnchor<PerContextResourceConsumerScope<TDataRepo>>();
            _dataRepoAnchor = new ThreadStaticAnchor<DataRepositoryBase>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("Metadata")]
        public string Metadata()
        {
            try
            {
                var metadataJsonString = ContextProvider.Metadata();
                return metadataJsonString;
            }
            finally
            {
                CleanupCurrentThread();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [HttpPost]
        [Route("SaveChanges")]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            using ( var activity = _logger.RestWriteInProgress() )
            {

                try
                {
                    SaveResult result;

                    using ( var context = _framework.NewUnitOfWork<TDataRepo>() )
                    {
                        _dataRepoAnchor.Current = (DataRepositoryBase)(object)context;
                        result = ContextProvider.SaveChanges(saveBundle);
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
                    CleanupCurrentThread();
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

        public void CleanupCurrentThread()
        {
            _dataRepoAnchor.Clear();
            _domainContextAnchor.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public BreezeContextProvider<TDataRepo> ContextProvider
        {
            get
            {
                return _contextProvider;
            }
        }
    }
}
