using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Core.DataObjects.Conventions;
using NWheels.Entities;

namespace NWheels.Core.DataObjects
{
    public class MetadataConventionSet
    {
        private readonly IEnumerable<IMetadataConvention> _metadataConventions;
        private readonly IEnumerable<IRelationalMappingConvention> _relationalMappingConventions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataConventionSet(
            IEnumerable<IMetadataConvention> metadataConventions,
            IEnumerable<IRelationalMappingConvention> relationalMappingConventions)
        {
            _metadataConventions = metadataConventions;
            _relationalMappingConventions = relationalMappingConventions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InjectCache(TypeMetadataCache cache)
        {
            foreach ( var convention in _metadataConventions )
            {
                convention.InjectCache(cache);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ApplyTypeConventions(TypeMetadataBuilder type, bool doPreview = true, bool doApply = true, bool doFinalize = true)
        {
            var shouldPreview = (!type.MetadataConventionsPreviewed) && (doPreview || doApply || doFinalize);
            var shouldApply = (!type.MetadataConventionsApplied) && (doApply || doFinalize);
            var shouldFinalize = (!type.MetadataConventionsFinalized) && (doFinalize);

            type.MetadataConventionsPreviewed |= ApplyMetadataConventionPhase(
                shouldPreview, 
                convention => convention.Preview(type));

            type.MetadataConventionsApplied |= ApplyMetadataConventionPhase(
                shouldApply, 
                convention => convention.Apply(type));

            type.MetadataConventionsFinalized |= ApplyMetadataConventionPhase(
                shouldFinalize, 
                convention => convention.Finalize(type));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ApplyRelationalMappingConventions(TypeMetadataBuilder type)
        {
            foreach ( var convention in _relationalMappingConventions )
            {
                convention.Preview(type);
            }

            foreach ( var convention in _relationalMappingConventions )
            {
                convention.Apply(type);
            }

            foreach ( var convention in _relationalMappingConventions )
            {
                convention.Finalize(type);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ApplyMetadataConventionPhase(bool condition, Action<IMetadataConvention> phaseAction)
        {
            if ( condition )
            {
                foreach ( var convention in _metadataConventions )
                {
                    phaseAction(convention);
                }

                return true;
            }

            return false;
        }
    }
}
