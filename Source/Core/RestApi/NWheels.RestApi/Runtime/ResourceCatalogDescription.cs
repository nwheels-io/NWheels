using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public class ResourceCatalogDescription
    {
        public ResourceCatalogDescription(string classifierUriPath, IEnumerable<IResourceDescription> resources)
        {
            this.ClassifierUriPath = classifierUriPath ?? throw new ArgumentNullException(nameof(classifierUriPath));
            this.Resources = resources?.ToArray() ?? throw new ArgumentNullException(nameof(resources));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string ClassifierUriPath { get; }
        public IEnumerable<IResourceDescription> Resources { get; }
    }
}
