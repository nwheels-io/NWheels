namespace NWheels.Hosting
{
    public interface INodeConfiguration
    {
        string ToLogString();
        string ApplicationName { get; set; }
        string NodeName { get; set; }
        string InstanceId { get; set; }
        string EnvironmentName { get; set; }
        string EnvironmentType { get; set; }
    }
}
