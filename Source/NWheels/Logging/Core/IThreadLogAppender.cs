namespace NWheels.Logging.Core
{
    public interface IThreadLogAppender
    {
        void StartThreadLog(ThreadTaskType taskType, ActivityLogNode rootActivity);
        void AppendLogNode(LogNode node);
        void AppendActivityNode(ActivityLogNode activity);
    }
}
