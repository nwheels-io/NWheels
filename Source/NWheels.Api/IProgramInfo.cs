namespace NWheels.Api
{
    public interface IProgramInfo
    {
        string EnvironmentType { get; }
        string EnvironmentName { get; }
        string ApplicationName { get; }
        string ServiceName { get; }
        string InstanceName { get; }
        int ReplicaId { get; }
    }
}