using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Authorization;
using NWheels.Configuration;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.Endpoints.Core;
using NWheels.Extensions;
using NWheels.Hosting;
using NWheels.Logging;
using NWheels.Processing.Messages;
using NWheels.Processing.Messages.Impl;

namespace NWheels.Stacks.MongoDb.Messages
{
    public class PollingMongoDbMessageQueueEndpoint : LifecycleEventListenerBase, IEndpoint, IMessageHandler<IMessageObject>
    {
        public const string EndpointName = "MongoDb.MessageQueue";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IFramework _framework;
        private readonly IConfig _config;
        private readonly ILogger _logger;
        private readonly ServiceBus _serviceBus;
        private MongoDatabase _database;
        private MongoCollection _collection;
        private CancellationTokenSource _cancellation;
        private Thread _pollingThread;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PollingMongoDbMessageQueueEndpoint(IFramework framework, IConfig config, ILogger logger, ServiceBus serviceBus)
        {
            _framework = framework;
            _config = config;
            _logger = logger;
            _serviceBus = serviceBus;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEndpoint

        public void PushMessage(ISession session, IMessageObject message)
        {
            var toHeader = message.TryGetHeader<MessageToHeader>();

            if (toHeader != null)
            {
                var fromHeader = message.TryGetHeader<MessageFromHeader>();
                WriteMessageToDb(message, fromHeader, toHeader);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name
        {
            get { return EndpointName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsPushSupported
        {
            get { return true; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimeSpan? SessionIdleTimeoutDefault
        {
            get { return null; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of LifecycleEventListenerBase

        public override void Load()
        {
            var databaseName = MongoUrl.Create(_config.ConnectionString).DatabaseName;
            _database = new MongoClient(_config.ConnectionString).GetServer().GetDatabase(databaseName);
            _collection = _database.GetCollection("MessageQueue");
            _collection.CreateIndex(new IndexKeysBuilder().Ascending("MessageId", "CreatedAt"));

            _serviceBus.SubscribeActor(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeActivated()
        {
            _cancellation = new CancellationTokenSource();
            _pollingThread = new Thread(() => {
                _framework.As<ICoreFramework>().RunThreadCode(
                    RunPollingThread, 
                    description: "PollingMongoDbMessageQueueEndpoint.RunPollingThread");
            });
            _pollingThread.Start();
            _logger.PollingThreadStarted();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeDeactivating()
        {
            _cancellation.Cancel();

            if (_pollingThread.Join(TimeSpan.FromSeconds(30)))
            {
                _logger.PollingThreadStopped();
            }
            else
            {
                _logger.PollingThreadDidNotStopInTimelyFashion();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<object>

        public void HandleMessage(IMessageObject message)
        {
            var toHeader = message.TryGetHeader<MessageToHeader>();

            if (toHeader != null)
            {
                var fromHeader = message.TryGetHeader<MessageFromHeader>();
                WriteMessageToDb(message, fromHeader, toHeader);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteMessageToDb(IMessageObject message, MessageFromHeader fromHeader, MessageToHeader toHeader)
        {
            var messageDocument = new BsonDocument();

            messageDocument["From"] = (
                fromHeader != null 
                ? BsonString.Create(fromHeader.Sender) 
                : BsonString.Create(GetThisEndpointName()));
            messageDocument["To"] = (
                toHeader != null  
                ? (BsonValue)BsonString.Create(toHeader.Recipient) 
                : (BsonValue)BsonNull.Value);
            messageDocument["MessageId"] = BsonBinaryData.Create(message.MessageId);
            messageDocument["CreatedAt"] = BsonDateTime.Create(message.CreatedAtUtc);
            messageDocument["BodyType"] = BsonString.Create(message.Body.GetType().FullName);

            var bodyDocument = new BsonDocument();

            using (var writer = BsonWriter.Create(bodyDocument))
            {
                BsonSerializer.Serialize(writer, message.Body.GetType(), message.Body);
                writer.Flush();
            }

            messageDocument["Body"] = bodyDocument;

            _collection.Insert(messageDocument, WriteConcern.Unacknowledged);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunPollingThread()
        {
            while (true)
            {
                if (_cancellation.Token.WaitHandle.WaitOne(_config.PollingInterval))
                {
                    return;
                }

                using (var threadLog = _logger.PollingMessages())
                {
                    try
                    {
                        PollMessages();
                    }
                    catch (Exception e)
                    {
                        threadLog.Fail(e);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PollMessages()
        {
            int dequeuedCount = 0;
            var messageQuery = CreateMessageQuery();

            while (dequeuedCount < _config.MaxMessagesPerPoll)
            {
                if (_cancellation.IsCancellationRequested)
                {
                    return;
                }

                var result = _collection.FindAndRemove(messageQuery);

                try
                {
                    if (result.ModifiedDocument != null && !_cancellation.IsCancellationRequested)
                    {
                        dequeuedCount++;

                        var message = TryOpenMessage(result.ModifiedDocument);

                        if (message != null)
                        {
                            _serviceBus.EnqueueMessage(message);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    _logger.DequeuedMessageFailed("???", Guid.Empty, e);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private FindAndRemoveArgs CreateMessageQuery()
        {
            var messageBodyTypes = _serviceBus.GetSubscribedMessageBodyTypes();
            var messageBodyTypeBsonList = messageBodyTypes.Select(type => BsonValue.Create(type.FullName)).ToArray();
            var findAndRemoveArgs = new FindAndRemoveArgs() {
                Query = Query.And(
                    Query.EQ("To", GetThisEndpointName()),
                    Query.In("BodyType", messageBodyTypeBsonList)
                ),
                SortBy = SortBy.Ascending("CreatedAt")
            };

            return findAndRemoveArgs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetThisEndpointName()
        {
            return _config.EndpointName ?? _framework.CurrentNode.NodeName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IMessageObject TryOpenMessage(BsonDocument messageDocument)
        {
            var messageId = Guid.Empty;
            var bodyTypeString = "???";

            try
            {
                messageId = messageDocument["MessageId"].AsGuid;
                bodyTypeString = messageDocument["BodyType"].AsString;
                var bodyType = Type.GetType(bodyTypeString, throwOnError: true);
                var messageObject = (IMessageObject)BsonSerializer.Deserialize(messageDocument["Body"].AsBsonDocument, bodyType);

                ReadMessageHeaders(messageDocument, messageObject);
                
                _logger.MessageDequeued(bodyTypeString, messageId);
                return messageObject;
            }
            catch (Exception e)
            {
                _logger.DequeuedMessageFailed(bodyTypeString, messageId, e);
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReadMessageHeaders(BsonDocument messageDocument, IMessageObject messageObject)
        {
            var fromHeaderValue = messageDocument["From"].AsString;
            var toHeaderValue = messageDocument["To"].AsString;

            if (!string.IsNullOrEmpty(fromHeaderValue))
            {
                messageObject.AddHeader(new MessageFromHeader(fromHeaderValue));
            }

            if (!string.IsNullOrEmpty(toHeaderValue))
            {
                messageObject.AddHeader(new MessageToHeader(toHeaderValue));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "MongoDb.MessageQueue")]
        public interface IConfig : IConfigurationSection
        {
            string EndpointName { get; set; }

            [PropertyContract.DefaultValue("00:00:01")]
            TimeSpan PollingInterval { get; set; }

            [PropertyContract.DefaultValue(100)]
            int MaxMessagesPerPoll { get; set; }

            [PropertyContract.Required]
            string ConnectionString { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ILogger : IApplicationEventLogger
        {
            [LogThread(ThreadTaskType.QueuedWorkItem)]
            ILogActivity PollingMessages();

            [LogVerbose]
            void CancellationRequestedExiting();

            [LogVerbose]
            void MessageDequeued(string type, Guid id);

            [LogError]
            void DequeuedMessageFailed(string type, Guid id, Exception error);

            [LogInfo]
            void PollingThreadStarted();

            [LogInfo]
            void PollingThreadStopped();

            [LogWarning]
            void PollingThreadDidNotStopInTimelyFashion();
        }
    }
}
