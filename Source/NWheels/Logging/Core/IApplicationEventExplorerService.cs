using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Logging.Core.ApplicationEventExplorer.RequestReply;

namespace NWheels.Logging.Core
{
    [ServiceContract(Namespace = FrameworkSoapNames.NamespaceUri, Name = "ApplicationEventExplorerService")]
    public interface IApplicationEventExplorerService
    {
        [OperationContract]
        QueryValueListReply QueryValueList(QueryValueListRequest request);
        
        [OperationContract]
        QueryEventChartReply QueryEventChart(QueryEventChartRequest request);

        [OperationContract]
        Stream StreamRealTimeEventChart(StreamRealTimeEventChartRequest request);

        [OperationContract]
        QueryEventListReply QueryEventList(QueryEventListRequest request);
        
        [OperationContract]
        QueryThreadLogListReply QueryThreadLogList(QueryThreadLogListRequest request);
        
        [OperationContract]
        Stream QueryThreadLogs(QueryThreadLogsRequest request);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace ApplicationEventExplorer.RequestReply
    {
        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class EventQuery
        {
            [DataMember]
            public EventQuerySelect Select { get; set; }
            [DataMember]
            public EventQueryFilter Filter { get; set; }
            [DataMember]
            public EventQueryOrderGroup Order { get; set; }
            [DataMember]
            public EventQueryOrderGroup Group { get; set; }
            [DataMember]
            public EventQueryTimeUnit TimeUnit { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri, Name = "EventQueryAggregation")]
        public class EventQuerySelect
        {
            [DataMember]
            public PropertySelectOperator? Timestamp { get; set; }
            [DataMember]
            public PropertySelectOperator? ApplicationName { get; set; }
            [DataMember]
            public PropertySelectOperator? EnvironmentName { get; set; }
            [DataMember]
            public PropertySelectOperator? EnvironmentType { get; set; }
            [DataMember]
            public PropertySelectOperator? MachineName { get; set; }
            [DataMember]
            public PropertySelectOperator? ClusterName { get; set; }
            [DataMember]
            public PropertySelectOperator? NodeName { get; set; }
            [DataMember]
            public PropertySelectOperator? NodeInstance { get; set; }
            [DataMember]
            public PropertySelectOperator? TaskType { get; set; }
            [DataMember]
            public PropertySelectOperator? LogLevel { get; set; }
            [DataMember]
            public PropertySelectOperator? LoggerName { get; set; }
            [DataMember]
            public PropertySelectOperator? MessageId { get; set; }
            [DataMember]
            public IDictionary<string, PropertySelectOperator> KeyValues { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri, Name = "EventQueryFilter")]
        public class EventQueryFilter
        {
            [DataMember]
            public DateTime FromUtc { get; set; }
            [DataMember]
            public DateTime UntilUtc { get; set; }
            [DataMember]
            public PropertyFilter<string> ApplicationName { get; set; }
            [DataMember]
            public PropertyFilter<string> EnvironmentName { get; set; }
            [DataMember]
            public PropertyFilter<string> EnvironmentType { get; set; }
            [DataMember]
            public PropertyFilter<string> MachineName { get; set; }
            [DataMember]
            public PropertyFilter<string> ClusterName { get; set; }
            [DataMember]
            public PropertyFilter<string> NodeName { get; set; }
            [DataMember]
            public PropertyFilter<string> NodeInstance { get; set; }
            [DataMember]
            public PropertyFilter<ThreadTaskType> TaskType { get; set; }
            [DataMember]
            public PropertyFilter<LogLevel> LogLevel { get; set; }
            [DataMember]
            public PropertyFilter<string> LoggerName { get; set; }
            [DataMember]
            public PropertyFilter<string> MessageId { get; set; }
            [DataMember]
            public IDictionary<string, PropertyFilter<string>> KeyValues { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri, Name = "EventQueryOrder")]
        public class EventQueryOrderGroup
        {
            [DataMember]
            public int? Timestamp { get; set; }
            [DataMember]
            public int? ApplicationName { get; set; }
            [DataMember]
            public int? EnvironmentName { get; set; }
            [DataMember]
            public int? EnvironmentType { get; set; }
            [DataMember]
            public int? MachineName { get; set; }
            [DataMember]
            public int? ClusterName { get; set; }
            [DataMember]
            public int? NodeName { get; set; }
            [DataMember]
            public int? NodeInstance { get; set; }
            [DataMember]
            public int? TaskType { get; set; }
            [DataMember]
            public int? LogLevel { get; set; }
            [DataMember]
            public int? LoggerName { get; set; }
            [DataMember]
            public int? MessageId { get; set; }
            [DataMember]
            public IDictionary<string, int> KeyValues { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum PropertyFilterOperator
        {
            Equal,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
            In,
            Contains,
            StartsWith,
            EndsWith
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum PropertySelectOperator
        {
            Value,
            Distinct,
            Min,
            Max,
            Average
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class PropertyFilter<T>
        {
            [DataMember]
            public PropertyFilterOperator Operator { get; set; }
            [DataMember]
            public bool Negation { get; set; }
            [DataMember]
            public IList<T> Values { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum EventQueryTimeUnit
        {
            Month,
            Day,
            Hour,
            Minute,
            Second
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region QueryLookups

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryValueListRequest
        {
            [DataMember]
            public EventQueryFilter Filter { get; set; }
            [DataMember]
            public bool ApplicationName { get; set; }
            [DataMember]
            public bool EnvironmentName { get; set; }
            [DataMember]
            public bool EnvironmentType { get; set; }
            [DataMember]
            public bool MachineName { get; set; }
            [DataMember]
            public bool ClusterName { get; set; }
            [DataMember]
            public bool NodeName { get; set; }
            [DataMember]
            public bool NodeInstance { get; set; }
            [DataMember]
            public bool TaskType { get; set; }
            [DataMember]
            public bool LogLevel { get; set; }
            [DataMember]
            public bool LoggerName { get; set; }
            [DataMember]
            public bool MessageId { get; set; }
            [DataMember]
            public IList<string> KeyValues { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryValueListReply
        {
            [DataMember]
            public IList<string> ApplicationName { get; set; }
            [DataMember]
            public IList<string> EnvironmentName { get; set; }
            [DataMember]
            public IList<string> EnvironmentType { get; set; }
            [DataMember]
            public IList<string> MachineName { get; set; }
            [DataMember]
            public IList<string> ClusterName { get; set; }
            [DataMember]
            public IList<string> NodeName { get; set; }
            [DataMember]
            public IList<string> NodeInstance { get; set; }
            [DataMember]
            public IList<ThreadTaskType> TaskType { get; set; }
            [DataMember]
            public IList<LogLevel> LogLevel { get; set; }
            [DataMember]
            public IList<string> LoggerName { get; set; }
            [DataMember]
            public IList<string> MessageId { get; set; }
            [DataMember]
            public IDictionary<string, IList<string>> KeyValues { get; set; }
        }

        #endregion 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region QueryEventChart

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryEventChartRequest
        {
            [DataMember]
            public EventQuerySelect Select { get; set; }
            [DataMember]
            public EventQueryFilter Filter { get; set; }
            [DataMember]
            public EventQueryOrderGroup Order { get; set; }
            [DataMember]
            public EventQueryOrderGroup Group { get; set; }
            [DataMember]
            public EventQueryTimeUnit TimeUnit { get; set; }
        }

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryEventChartReply
        {
        }

        #endregion 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region QueryEventChart

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class StreamRealTimeEventChartRequest
        {
            [DataMember]
            public EventQuerySelect Aggregation { get; set; }
            [DataMember]
            public EventQueryFilter Filter { get; set; }
            [DataMember]
            public EventQueryOrderGroup Order { get; set; }
            [DataMember]
            public EventQueryOrderGroup Group { get; set; }
            [DataMember]
            public EventQueryTimeUnit TimeUnit { get; set; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region QueryEventList

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryEventListRequest
        {
            [DataMember]
            public EventQueryFilter Query { get; set; }
            [DataMember]
            public EventQueryOrderGroup Order { get; set; }
            [DataMember]
            public EventQuerySelect Aggregation { get; set; }
        }

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryEventListReply
        {
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region QueryThreadLogList

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryThreadLogListRequest
        {
            [DataMember]
            public EventQueryFilter Query { get; set; }
        }

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryThreadLogListReply
        {
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region QueryThreadLog

        [DataContract(Namespace = FrameworkSoapNames.NamespaceUri)]
        public class QueryThreadLogsRequest
        {
            [DataMember]
            public EventQueryFilter Query { get; set; }
        }

        #endregion
    }
}
