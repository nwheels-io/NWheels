using System;
using System.Collections.Generic;
using System.Linq;

namespace NWheels.RestApi.Api
{
    public class ResourceCatalog
    {
        public ResourceCatalog(string baseUriPath, IEnumerable<Type> resources, IEnumerable<Type> protocols)
        {
            BaseUriPath = baseUriPath ?? throw new ArgumentNullException(nameof(baseUriPath));
            Resources = resources?.ToArray() ?? throw new ArgumentNullException(nameof(resources));
            Protocols = protocols?.ToArray() ?? throw new ArgumentNullException(nameof(protocols));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string BaseUriPath { get; }
        public IEnumerable<Type> Resources { get; }
        public IEnumerable<Type> Protocols { get; }
    }
}
