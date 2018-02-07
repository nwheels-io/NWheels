using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Injection;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public abstract class CompiledResourceCatalogBase : ICompiledResourceCatalog
    {
        private ResourceCatalogDescription _description = null;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected CompiledResourceCatalogBase(string baseUriPath)
        {
            this.BaseUriPath = (baseUriPath ?? throw new ArgumentNullException(nameof(baseUriPath))).TrimEnd('/');
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ResourceCatalogDescription GetDescription()
        {
            if (_description == null)
            {
                _description = BuildDescription();
            }

            return _description;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void PopulateRouter(IDictionary<string, ProtocolBindingMap> bindingMapByProtocol);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string BaseUriPath { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string GetResourceUriPath(Type resourceType)
        {
            return BaseUriPath + '/' + resourceType.FriendlyName();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract ResourceCatalogDescription BuildDescription();
    }
}
