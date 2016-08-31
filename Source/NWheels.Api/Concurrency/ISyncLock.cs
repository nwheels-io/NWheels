namespace NWheels.Api.Concurrency
{
    public interface ISyncLock
    {
        string ResourceName { get; }
    }
}
