namespace NWheels.Logging.Core
{
    internal interface IThreadRegistry
    {
        void ThreadStarted(ThreadLog threadLog);
        void ThreadFinished(ThreadLog threadLog);
        ThreadLog[] GetRunningThreads();
    }
}
