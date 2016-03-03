namespace NWheels.Processing
{
    public interface ICommandProcessedEvent : IPushEvent
    {
        int CommandIndex { get; set; }
        CommandStatus Status { get; set; }
        string FaultCode { get; set; }
        string FaultSubCode { get; set; }
        string FaultReason { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CommandStatus
    {
        Completed,
        Failed,
        TimedOut,
        Cancelled
    }
}
