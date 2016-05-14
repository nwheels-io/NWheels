using NWheels.Hosting;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public static class DbNamingConvention
    {
        public static readonly string CollectionNamePrefix = "System.Logs.";
        public static readonly string DailySummaryCollectionNameSuffix = ".DailySummary";
        public static readonly string LogMessageCollectionNameSuffix = ".LogMessage";
        public static readonly string ThreadLogCollectionNameSuffix = ".ThreadLog";
        public static readonly string DefaultDatabaseName = "nwheels_log";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetDailySummaryCollectionName(string environmentName)
        {
            return CollectionNamePrefix + environmentName + DailySummaryCollectionNameSuffix;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetLogMessageCollectionName(string environmentName)
        {
            return CollectionNamePrefix + environmentName + LogMessageCollectionNameSuffix;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetThreadLogCollectionName(string environmentName)
        {
            return CollectionNamePrefix + environmentName + ThreadLogCollectionNameSuffix;
        }
    }
}