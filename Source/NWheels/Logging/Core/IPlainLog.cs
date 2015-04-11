namespace NWheels.Logging.Core
{
    public interface IPlainLog
    {
        void ConfigureConsoleOutput();
        void ConfigureWindowsEventLogOutput(string logName, string sourceName);
        void LogNode(NWheels.Logging.LogNode node);
        void LogActivity(NWheels.Logging.ActivityLogNode activity);
        void Debug(string format, params object[] args);
        void Info(string format, params object[] args);
        void Warning(string format, params object[] args);
        void Error(string format, params object[] args);
        void Critical(string format, params object[] args);
    }
}
