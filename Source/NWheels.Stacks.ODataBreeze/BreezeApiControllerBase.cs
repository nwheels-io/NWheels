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
using Breeze.ContextProvider;
using Breeze.WebApi2;
using Newtonsoft.Json.Linq;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    [BreezeController]
    [EnableCors("*", "*", "*")]
    public abstract class BreezeApiControllerBase<TDataRepo> : ApiController
        where TDataRepo : class, IApplicationDataRepository
    {
        private readonly BreezeContextProvider<TDataRepo> _contextProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected BreezeApiControllerBase(IFramework framework, ITypeMetadataCache metadataCache)
        {
            _contextProvider = new BreezeContextProvider<TDataRepo>(framework, metadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("Metadata")]
        public string Metadata()
        {
            var metadataJsonString = _contextProvider.Metadata();
            return metadataJsonString;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [HttpPost]
        [Route("SaveChanges")]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _contextProvider.SaveChanges(saveBundle);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public BreezeContextProvider<TDataRepo> ContextProvider
        {
            get { return _contextProvider; }
        }
    }
}
