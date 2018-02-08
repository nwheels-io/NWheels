using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Kernel.Api.Injection
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
}
