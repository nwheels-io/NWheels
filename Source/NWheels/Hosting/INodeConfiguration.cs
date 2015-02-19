namespace NWheels.Hosting
{
    public interface INodeConfiguration
    {
        string ToLogString();
        string ApplicationName { get; }
        string NodeName { get; }
        string InstanceId { get; }
        string EnvironmentName { get; }
        string EnvironmentType { get; }
    }
}
