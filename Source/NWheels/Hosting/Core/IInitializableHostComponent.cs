namespace NWheels.Hosting.Core
{
    public interface IInitializableHostComponent
    {
        void Initializing();
        void Configured();
    }
}
