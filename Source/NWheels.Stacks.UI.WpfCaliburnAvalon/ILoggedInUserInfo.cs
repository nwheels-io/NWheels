namespace NWheels.Stacks.UI.WpfCaliburnAvalon
{
    public interface ILoggedInUserInfo
    {
        string UserId { get; }
        string LoginName { get; }
        string FullName { get; }
        string Role { get; }
        string SerializedWorkspace { get; }
    }
}
