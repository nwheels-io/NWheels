namespace NWheels.Kernel.Api.Injection
{
    public interface IFeatureLoaderWithPhaseExtension : IFeatureLoader
    {
        IFeatureLoaderPhaseExtension PhaseExtension { get; }
    }
}
