using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    public class DomainContextMetadataCache : IDomainContextMetadataCache
    {
        private readonly ConcurrentDictionary<Type, DomainContextMetadataBuilder> _metaContextByType = 
            new ConcurrentDictionary<Type, DomainContextMetadataBuilder>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDomainContextMetadataCache

        public IDomainContextMetadata GetDomainContextMetadata(Type contextType)
        {
            return _metaContextByType.GetOrAdd(contextType, BuildDomainContextMetadata);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private DomainContextMetadataBuilder BuildDomainContextMetadata(Type contextType)
        {
            throw new NotImplementedException();
        }
    }
}
