using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using System.Collections.Immutable;

namespace NWheels.Microservices
{
    public class CompositeFeatureLoader : IFeatureLoader
    {
        private readonly ImmutableArray<IFeatureLoader> _containedFeatures;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompositeFeatureLoader(params IFeatureLoader[] containedFeatures)
        {
            _containedFeatures = containedFeatures.ToImmutableArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CompileComponents(IComponentContainer existingComponents)
        {
            foreach (var feature in _containedFeatures)
            {
                feature.CompileComponents(existingComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            foreach (var feature in _containedFeatures)
            {
                feature.ContributeAdapterComponents(existingComponents, newComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            foreach (var feature in _containedFeatures)
            {
                feature.ContributeCompiledComponents(existingComponents, newComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            foreach (var feature in _containedFeatures)
            {
                feature.ContributeComponents(existingComponents, newComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ContributeConfigSections(IComponentContainerBuilder newComponents)
        {
            foreach (var feature in _containedFeatures)
            {
                feature.ContributeConfigSections(newComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ContributeConfiguration(IComponentContainer existingComponents)
        {
            foreach (var feature in _containedFeatures)
            {
                feature.ContributeConfiguration(existingComponents);
            }
        }
    }
}
