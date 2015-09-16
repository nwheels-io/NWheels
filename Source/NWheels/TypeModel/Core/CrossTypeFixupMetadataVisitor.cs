using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects.Core
{
    /// <summary>
    /// Refreshes cross-type references in order to make sure no stale references are used. 
    /// </summary>
    /// <remarks>
    /// Through metadata creation process, conventions are applied to TypeMetadataBuilder instances. A convention can extend type contract, which causes a new
    /// TypeMetadataBuilder instance to be built for the contract. That is, there may be multiple instances of TypeMetadataBuilder per same contract, where one 
    /// that created last is the correct. Cross-type references resolved for circular dependencies, thus may contain stale references to non-up-to-date
    /// TypeMetadataBuilder instances. This visitor fixes cross-type references to point to up-to-date instances of TypeMetadataBuilder.
    /// </remarks>
    public class CrossTypeFixupMetadataVisitor : TypeMetadataVisitorBase
    {
        private readonly TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrossTypeFixupMetadataVisitor(TypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TypeMetadataBuilder VisitType(TypeMetadataBuilder metadata)
        {
            if ( metadata.BaseType != null )
            {
                metadata.BaseType = (TypeMetadataBuilder)_metadataCache.GetTypeMetadata(metadata.BaseType.ContractType);
                metadata.EnsureBasePropertiesInherited();
            }

            if ( metadata.DerivedTypes != null )
            {
                for ( int i = 0 ; i < metadata.DerivedTypes.Count ; i++ )
                {
                    metadata.DerivedTypes[i] = (TypeMetadataBuilder)_metadataCache.GetTypeMetadata(metadata.DerivedTypes[i].ContractType);
                }
            }

            return metadata;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override RelationMetadataBuilder VisitRelation(RelationMetadataBuilder metadata)
        {
            if ( metadata != null && metadata.RelatedPartyType != null )
            {
                metadata.RelatedPartyType = (TypeMetadataBuilder)_metadataCache.GetTypeMetadata(metadata.RelatedPartyType.ContractType);

                if ( metadata.RelatedPartyKey != null )
                {
                    metadata.RelatedPartyKey = metadata.RelatedPartyType.AllKeys.First(key => key.Name == metadata.RelatedPartyKey.Name);
                }

                if ( metadata.InverseProperty != null )
                {
                    if ( metadata.InverseProperty.ContractPropertyInfo != null )
                    {
                        metadata.InverseProperty = 
                            (PropertyMetadataBuilder)metadata.RelatedPartyType.GetPropertyByDeclaration(metadata.InverseProperty.ContractPropertyInfo);
                    }
                    else
                    {
                        metadata.InverseProperty = 
                            (PropertyMetadataBuilder)metadata.RelatedPartyType.GetPropertyByName(metadata.InverseProperty.Name);
                    }
                }
            }

            return metadata;
        }
    }
}
