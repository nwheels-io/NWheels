using System;
using System.Collections.Generic;
using NWheels.DataObjects;

namespace NWheels.TypeModel.Core
{
    public class DomainContextMetadataBuilder : IDomainContextMetadata
    {
        private readonly Type _contractType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainContextMetadataBuilder(Type contractType)
        {
            _contractType = contractType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDomainContextMetadata

        public Type GetImplementationTypeBy(Type facoryType)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Type> GetAllImplementations()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Type ContractType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyCollection<IDomainContextEntityMetadata> IDomainContextMetadata.Entities 
        {
            get { throw new NotImplementedException(); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        IReadOnlyCollection<ITypeMetadata> IDomainContextMetadata.Types 
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyCollection<IDomainContextEntityMetadata> Entities { get; private set; }
        public IReadOnlyCollection<ITypeMetadata> Types { get; private set; }
    }
}
