using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Extensions;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public class DailySummaryRecord : LogRecordBase
    {
        public DailySummaryRecord()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DailySummaryRecord(LogNode node)
        {
            this.Id = GetRecordId(node);
            this.Date = node.GetUtcTimestamp().Date;
            this.Hour = new Dictionary<string, int>(capacity: 24);
            this.Minute = new Dictionary<string, Dictionary<string, int>>(capacity: 24);

            Initialize(node);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DailySummaryRecord(LogNode node, DateTime date)
        {
            this.Id = GetRecordId(node, date);
            this.Date = date;
            this.Hour = _s_emptyHourDictionary;
            this.Minute = _s_emptyMinuteDictionary;

            Initialize(node);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Increment(LogNode node)
        {
            var time = node.GetUtcTimestamp().TimeOfDay;
            var hourIndex = _s_hourStringIndex[time.Hours];
            var minuteIndex = _s_minuteStringIndex[time.Minutes];

            int currentHourValue;
            this.Hour.TryGetValue(hourIndex, out currentHourValue);
            this.Hour[hourIndex] = currentHourValue + 1;

            int currentMinuteValue;
            var minuteWithinHour = this.Minute.GetOrAdd(hourIndex, key => new Dictionary<string, int>(capacity: 60));
            minuteWithinHour.TryGetValue(minuteIndex, out currentMinuteValue);
            minuteWithinHour[minuteIndex] = currentMinuteValue + 1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildIncrementUpsert(BulkWriteOperation bulkWrite)
        {
            var update = new UpdateBuilder();

            foreach ( var hour in this.Hour )
            {
                if ( hour.Value > 0 )
                {
                    var key = hour.Key;
                    var value = hour.Value;
                    update.Inc("Hour." + key, value);
                }
            }

            foreach ( var hour in this.Minute )
            {
                foreach ( var minute in hour.Value )
                {
                    if ( minute.Value > 0 )
                    {
                        var hourKey = hour.Key;
                        var minuteKey = minute.Key;
                        var value = minute.Value;
                        update.Inc("Minute." + hourKey + "." + minuteKey, value);
                    }
                }
            }

            var query = GetRecordQuery();
            bulkWrite.Find(query).Upsert().Update(update);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMongoQuery GetRecordQuery()
        {
            var query = Query.And(
                Query<DailySummaryRecord>.EQ(x => x.Id, this.Id),
                Query<DailySummaryRecord>.EQ(x => x.Date, this.Date),
                Query<DailySummaryRecord>.EQ(x => x.MachineName, this.MachineName),
                Query<DailySummaryRecord>.EQ(x => x.NodeName, this.NodeName),
                Query<DailySummaryRecord>.EQ(x => x.NodeInstance, this.NodeInstance.NullIfEmpty()),
                Query<DailySummaryRecord>.EQ(x => x.Level, this.Level),
                Query<DailySummaryRecord>.EQ(x => x.Logger, this.Logger),
                Query<DailySummaryRecord>.EQ(x => x.MessageId, this.MessageId),
                Query<DailySummaryRecord>.EQ(x => x.ExceptionType, this.ExceptionType));

            return query;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonId]
        public string Id { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonDateTimeOptions(DateOnly = true, Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonRepresentation(BsonType.String)]
        public LogLevel Level { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Logger { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MessageId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExceptionType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, int> Hour { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, Dictionary<string, int>> Minute { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Initialize(LogNode node)
        {
            var currentNode = node.ThreadLog.Node;

            this.MachineName = MongoDbThreadLogPersistor.MachineName;
            this.NodeName = currentNode.NodeName;
            this.NodeInstance = currentNode.InstanceId.NullIfEmpty();
            this.MessageId = node.MessageId;
            this.Level = node.Level;

            var periodPosition = node.MessageId.IndexOf('.');
            if ( periodPosition > 0 )
            {
                this.Logger = node.MessageId.Substring(0, periodPosition);
            }

            this.ExceptionType = node.ExceptionTypeName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string[] _s_hourStringIndex = new[] {
            "0" , "1" , "2" , "3" , "4" , "5" , "6" , "7" , "8" , "9" , "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly string[] _s_minuteStringIndex = new[] {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", 
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", 
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59"
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<string, int> _s_emptyHourDictionary;
        private static readonly Dictionary<string, Dictionary<string, int>> _s_emptyMinuteDictionary;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static DailySummaryRecord()
        {
            var emptyCountByMinute = new Dictionary<string, int>(capacity: 60);

            for ( int minute = 0; minute < 60; minute++ )
            {
                emptyCountByMinute[_s_minuteStringIndex[minute]] = 0;
            }

            _s_emptyHourDictionary = new Dictionary<string, int>(capacity: 24);
            _s_emptyMinuteDictionary = new Dictionary<string, Dictionary<string, int>>(capacity: 24);

            for ( int hour = 0 ; hour < 24 ; hour++ )
            {
                _s_emptyHourDictionary[_s_hourStringIndex[hour]] = 0;
                _s_emptyMinuteDictionary[_s_hourStringIndex[hour]] = emptyCountByMinute;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetRecordId(LogNode node)
        {
            return GetRecordId(node, node.GetUtcTimestamp().Date);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetRecordId(LogNode node, DateTime date)
        {
            return (
                date.ToString("yyyyMMdd") + "/" +
                MongoDbThreadLogPersistor.MachineName + "/" +
                node.ThreadLog.Node.NodeName + "/" +
                node.ThreadLog.Node.InstanceId + "/" +
                node.Level.ToString() + "/" +
                node.MessageId + "/" + 
                node.ExceptionTypeName);
        }
    }
}
