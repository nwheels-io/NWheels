using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities;

namespace NWheels.DataObjects.Core
{
    public class MetadataConventionSet
    {
        private readonly IMetadataConvention[] _metadataConventions;
        private readonly IRelationalMappingConvention[] _relationalMappingConventions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataConventionSet(
            Pipeline<IMetadataConvention> metadataConventions,
            Pipeline<IRelationalMappingConvention> relationalMappingConventions)
        {
            _metadataConventions = metadataConventions.ToArray();
            _relationalMappingConventions = relationalMappingConventions.ToArray();
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
            var orderedConventions = _relationalMappingConventions.Reverse().ToArray();

            type.SafeGetRelationalMapping();

            foreach ( var convention in orderedConventions )
            {
                convention.Preview(type);
            }

            foreach ( var convention in orderedConventions )
            {
                convention.Apply(type);
            }

            foreach ( var convention in orderedConventions )
            {
                convention.Finalize(type);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IMetadataConvention> MetadataConventions
        {
            get { return _metadataConventions; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IRelationalMappingConvention> RelationalMappingConventions
        {
            get { return _relationalMappingConventions; }
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
