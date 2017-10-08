using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Kernel.Runtime.Injection
{
    public interface IFeatureLoaderPhaseExtension
    {
        void BeforeContributeConfigSections(IComponentContainer components);
        void BeforeContributeConfiguration(IComponentContainer components);
        void BeforeContributeComponents(IComponentContainer components);
        void BeforeContributeAdapterComponents(IComponentContainer comcomponents);
        void BeforeCompileComponents(IComponentContainer comcomponents);
        void BeforeContributeCompiledComponents(IComponentContainer comcomponents);
        void AfterContributeCompiledComponents(IComponentContainer comcomponents);
    }

    public interface IFeatureLoaderWithPhaseExtension : IFeatureLoader
    {
        IFeatureLoaderPhaseExtension PhaseExtension { get; }
    }
}
