namespace NWheels.Processing
{
    public interface IServerCommand
    {
        int Index { get; set; }
        bool ShouldNotifyCompletion { get; set; }
    }
}
