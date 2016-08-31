namespace NWheels.Api.Logging
{
    public enum ThreadTaskType
    {
        Unknown,
        BatchWork,
        StartupShutdown,
        QueuedWorkItem,
        ScheduledJob,
        ApiRequest
    }
}