using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MongoBulkUpdate461
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args[0];
            var dbName = args[1];
            var collectionName = args[2];
            var filterJson = args[3];
            var fieldName = args[4];
            var updateOperator = Enum.Parse(typeof(UpdateOperator), args[5]);
            var updateValue = decimal.Parse(args[6]);
            var useBulkApi = (args.Length >= 8 && args[7].Equals("BULK", StringComparison.OrdinalIgnoreCase));

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            var fieldDefinition = new StringFieldDefinition<BsonDocument, decimal>(fieldName);
            var filterDefinition = new JsonFilterDefinition<BsonDocument>(filterJson);

            long totalDocuments = collection.Count(filterDefinition);
            long doneDocuments = 0;

            Console.WriteLine($"Updating {totalDocuments:#,##0} documents.");

            try
            {
                if (useBulkApi)
                {
                    UpdateUsingBulkApi();
                }
                else
                {
                    UpdateUsingCursorLoop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            void UpdateUsingCursorLoop()
            {
                var bulk = new List<ReplaceOneModel<BsonDocument>>(capacity: 1000);

                var task = collection.Find(filterDefinition).ForEachAsync(document => {
                    var bsonValue = document[fieldName];
                    var value = bsonValue.IsString
                        ? decimal.Parse(bsonValue.AsString)
                        : bsonValue.IsInt64
                            ? bsonValue.AsInt64 / 1000000
                            : bsonValue.AsDecimal;

                    switch (updateOperator)
                    {
                        case UpdateOperator.Set:
                            value = updateValue;
                            break;
                        case UpdateOperator.Add:
                            value += updateValue;
                            break;
                        case UpdateOperator.Subtract:
                            value -= updateValue;
                            break;
                        case UpdateOperator.Multiply:
                            value *= updateValue;
                            break;
                        case UpdateOperator.Divide:
                            value /= updateValue;
                            break;
                        default:
                            return;
                    }

                    if (bsonValue.IsString)
                    {
                        document[fieldName] = BsonValue.Create(value.ToString());
                    }
                    else if (bsonValue.IsInt64)
                    {
                        document[fieldName] = BsonValue.Create(value * 1000000L);
                    }
                    else
                    {
                        document[fieldName] = BsonValue.Create(value);
                    }

                    bulk.Add(new ReplaceOneModel<BsonDocument>(
                        filter: new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument(new BsonElement("_id", document["_id"]))),
                        replacement: document));

                    if (bulk.Count >= 1000)
                    {
                        collection.BulkWrite(bulk, new BulkWriteOptions { IsOrdered = false });
                        bulk.Clear();
                        doneDocuments += 1000;
                        Console.Write($"\rdone: {doneDocuments:#,##0}");
                    }
                });

                task.Wait();

                if (bulk.Count > 0)
                {
                    collection.BulkWrite(bulk, new BulkWriteOptions { IsOrdered = false });
                    doneDocuments += bulk.Count;
                }

                Console.WriteLine($"\r{doneDocuments:#,##0} documents updated.");
            }

            void UpdateUsingBulkApi()
            {
                var updateBuilder = new UpdateDefinitionBuilder<BsonDocument>();
                UpdateDefinition<BsonDocument> update;

                switch (updateOperator)
                {
                    case UpdateOperator.Set:
                        update = updateBuilder.Set<decimal>(new StringFieldDefinition<BsonDocument, decimal>(fieldName), updateValue);
                        break;
                    case UpdateOperator.Add:
                        update = updateBuilder.Inc<decimal>(new StringFieldDefinition<BsonDocument, decimal>(fieldName), updateValue);
                        break;
                    case UpdateOperator.Subtract:
                        update = updateBuilder.Inc<decimal>(new StringFieldDefinition<BsonDocument, decimal>(fieldName), -updateValue);
                        break;
                    case UpdateOperator.Multiply:
                        update = updateBuilder.Mul<decimal>(new StringFieldDefinition<BsonDocument, decimal>(fieldName), updateValue);
                        break;
                    case UpdateOperator.Divide:
                        update = updateBuilder.Mul<decimal>(new StringFieldDefinition<BsonDocument, decimal>(fieldName), 1 / updateValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("UpdateOperator");
                }

                var bulkOperations = new WriteModel<BsonDocument>[] {
                new UpdateManyModel<BsonDocument>(filterDefinition, update)
            };

                var result = collection.BulkWrite(bulkOperations, new BulkWriteOptions { IsOrdered = false });
                Console.WriteLine(JsonConvert.SerializeObject(result));
            }
        }
    }
}